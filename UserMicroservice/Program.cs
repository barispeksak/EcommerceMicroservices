using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer; 
using FluentValidation;
using FluentValidation.AspNetCore;
using UserMicroservice.Data;
using UserMicroservice.Dtos;
using UserMicroservice.Service.Mapping;
using UserMicroservice.Service.Validation;
using UserMicroservice.Service.Interfaces;
using UserMicroservice.Service.Services;
using UserMicroservice.Data.Repositories;
using Serilog;
using UserMicroservice.Api;
using UserMicroservice.Http;
using UserMicroservice.Middleware;
using MongoDB.Driver; // <-- Ekle!
using MassTransit;


var builder = WebApplication.CreateBuilder(args);
// ---------- DB ----------
builder.Services.AddDbContext<UserDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// ---------- Validation ----------
builder.Services.AddControllers()
                .AddJsonOptions(options =>
                    {
                        options.JsonSerializerOptions.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
                    });
builder.Services.AddFluentValidationAutoValidation()
                .AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssemblyContaining<CreateUserDtoValidator>();
builder.Services.AddScoped<IValidator<UserDto>, UserDtoValidator>();
var config   = builder.Configuration;      // ① DI dışı yedek referans

builder.Services.AddMassTransit(x =>
{
    // x.AddConsumer<...>();   // varsa consumer kayıtları

    x.UsingRabbitMq((ctx, busCfg) =>
    {
        var rmq = config.GetSection("RabbitMQ");   // ② ayarları buradan oku
        busCfg.Host(rmq["Host"], "/", h =>
        {
            h.Username(rmq["Username"]);
            h.Password(rmq["Password"]);
        });

        busCfg.ConfigureEndpoints(ctx);            // ③ queue’lar otomatik
    });
});


// ---------- AutoMapper ----------
builder.Services.AddAutoMapper(typeof(UserProfile));

// ---------- Services & Repos ----------
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddSingleton<UserActionLogger>();

/* ---------- Serilog ---------- */
builder.Host.UseSerilog((ctx, lc) => lc
    .ReadFrom.Configuration(ctx.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("ServiceName", "UserMicroservice")
    .WriteTo.Console());

/* ---------- MongoDB DI ---------- */
builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var cfg = sp.GetRequiredService<IConfiguration>();
    var cs  = cfg["MongoDb:ConnectionString"]
              ?? throw new InvalidOperationException("MongoDb:ConnectionString not found!");
    return new MongoClient(cs);
});

builder.Services.AddSingleton<IMongoDatabase>(sp =>
{
    var cfg = sp.GetRequiredService<IConfiguration>();
    var db  = cfg["MongoDb:Database"]
              ?? throw new InvalidOperationException("MongoDb:Database not found!");
    return sp.GetRequiredService<IMongoClient>().GetDatabase(db);
});

/* ---------- JWT Authentication ---------- */
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
       .AddJwtBearer(o =>
       {
           // Authority: token’ı basan AuthService’in URL’i
           o.Authority = "http://authmicroservice"; // docker internal DNS
           o.RequireHttpsMetadata = false;          // dev ortamı
           // Eğer ek Audience kontrolü istersen:
           // o.Audience = "usermicroservice";
       });

// ---------- HttpClient (UserAddress) ----------
builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<CorrelationIdDelegatingHandler>();
builder.Services.AddHttpClient<IUserAddressApiClient, UserAddressApiClient>(c =>
{
    c.BaseAddress = new Uri("http://useraddressmicroservice");
})
.AddHttpMessageHandler<CorrelationIdDelegatingHandler>();


builder.Services.AddAuthorization();


/* ---------- Swagger (dev) ---------- */
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

/* ---------- Middleware Pipeline ---------- */
// Correlation-Id ekle / kopyala
app.UseMiddleware<CorrelationIdMiddleware>();

app.UseRouting();
app.UseSerilogRequestLogging();
app.UseAuthentication();
app.UseAuthorization();


app.UseSwagger();
app.UseSwaggerUI();


app.MapControllers();

app.Run();
