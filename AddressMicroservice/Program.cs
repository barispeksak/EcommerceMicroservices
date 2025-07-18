using Microsoft.EntityFrameworkCore;
using AddressMicroservice.Data;
using AddressMicroservice.Data.Repositories;
using AddressMicroservice.Service.Interfaces;
using AddressMicroservice.Service.Services;
using AddressMicroservice.Service.Mapping;
using FluentValidation;
using FluentValidation.AspNetCore;
using AddressMicroservice.Service.Validation;
using Serilog;
using MongoDB.Driver; 
using AddressMicroservice.Middleware;
using AddressMicroservice.Http;
using MassTransit;

var builder = WebApplication.CreateBuilder(args);

// ───── Serilog ─────────────────────────────────────────────
const string serviceName = "AddressMicroservice";
builder.Host.UseSerilog((ctx, lc) => lc
    .ReadFrom.Configuration(ctx.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("ServiceName", serviceName)
    .WriteTo.Console());

builder.Services.AddSingleton<IMongoClient>(_ =>
    new MongoClient("mongodb://mongo:27017"));          // docker-compose'da "mongo"

builder.Services.AddSingleton<IMongoDatabase>(sp =>
    sp.GetRequiredService<IMongoClient>()
      .GetDatabase("ECommerceLogs"));

builder.Services.AddSingleton<AddressActionLogger>();

// ───── Controllers / Validation ────────────────────────────
builder.Services.AddControllers();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssemblyContaining<CreateAddressDtoValidator>();

// ───── Entity Framework ────────────────────────────────────
builder.Services.AddDbContext<AddressDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// ───── AutoMapper & DI ─────────────────────────────────────
builder.Services.AddAutoMapper(typeof(MappingProfile));
builder.Services.AddScoped<IAddressRepository, AddressRepository>();
builder.Services.AddScoped<IAddressService,    AddressService>();

// ───── Correlation-Id için altyapı ─────────────────────────
builder.Services.AddHttpContextAccessor();                      // ← gerekli
builder.Services.AddTransient<CorrelationIdDelegatingHandler>(); // (isteğe bağlı)

// örnek outbound HttpClient — başka servise çağrı yapıyorsanız
//builder.Services.AddHttpClient("SomeOtherService", c =>
//{
//    c.BaseAddress = new Uri("http://otherservice");
//})
//.AddHttpMessageHandler<CorrelationIdDelegatingHandler>();

// ───── Swagger / CORS ───────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddPolicy("MicroservicePolicy",
        policy => policy.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader());
});

var config = builder.Configuration;      // ① DI dışı yedek referans

builder.Services.AddMassTransit(x =>
{
    // x.AddConsumer<...>();   // varsa consumer kayıtları

    x.UsingRabbitMq((ctx, busCfg) =>
    {
        var rmq = config.GetSection("RabbitMQ");   // ② ayarları buradan oku
        busCfg.Host(rmq["Host"], "/", h =>
        {
            // Null kontrolü ile güvenli atama
            var username = rmq["Username"];
            var password = rmq["Password"];
            
            if (!string.IsNullOrEmpty(username))
                h.Username(username);
            
            if (!string.IsNullOrEmpty(password))
                h.Password(password);
        });

        busCfg.ConfigureEndpoints(ctx);            // ③ queue'lar otomatik
    });
});

var app = builder.Build();

// ───── Pipeline ────────────────────────────────────────────
app.UseMiddleware<CorrelationIdMiddleware>();   // ❶ ilk loglayıcı bu olsun

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsDevelopment())
    app.UseHttpsRedirection();

app.UseCors("MicroservicePolicy");
app.UseSerilogRequestLogging();                 // ❷ Serilog’un request log’u
app.UseAuthorization();
app.MapControllers();

app.Run();