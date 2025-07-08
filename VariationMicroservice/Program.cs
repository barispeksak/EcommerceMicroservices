/*****************************************************
 *  Variation Microservice – Program.cs  (FULL)
 ****************************************************/
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;
using MongoDB.Driver;
using FluentValidation;
using FluentValidation.AspNetCore;

using VariationMicroservice.Data;
using VariationMicroservice.Data.Repositories;
using VariationMicroservice.Service.Interfaces;
using VariationMicroservice.Service.Services;
using VariationMicroservice.Service.Mapping;
using VariationMicroservice.Service.Validation;      // örnek validator’lar
using VariationMicroservice.Http;                   // CorrelationIdDelegatingHandler
using VariationMicroservice.Middleware;            // CorrelationIdMiddleware

var builder = WebApplication.CreateBuilder(args);

/*──────────────────────────────────────────────
  1. Serilog
  ─────────────────────────────────────────────*/
const string serviceName = "VariationMicroservice";
builder.Host.UseSerilog((ctx, lc) => lc
    .ReadFrom.Configuration(ctx.Configuration)     // appsettings*.json
    .Enrich.FromLogContext()
    .Enrich.WithProperty("ServiceName", serviceName)
    .WriteTo.Console());

/*──────────────────────────────────────────────
  2. MongoDB – log koleksiyonu
  ─────────────────────────────────────────────*/
builder.Services.AddSingleton<IMongoClient>(_ =>
    new MongoClient("mongodb://mongo:27017"));      // docker-compose’ta container adı “mongo”

builder.Services.AddSingleton<IMongoDatabase>(sp =>
    sp.GetRequiredService<IMongoClient>()
      .GetDatabase("ECommerceLogs"));

// Address tarafındaki “AddressActionLogger” benzeri bir sınıfınız varsa:
builder.Services.AddSingleton<VariationActionLogger>();

/*──────────────────────────────────────────────
  3. Entity Framework – PostgreSQL
  ─────────────────────────────────────────────*/
builder.Services.AddDbContext<VariationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

/*──────────────────────────────────────────────
  4. Correlation-Id Altyapısı
  ─────────────────────────────────────────────*/
builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<CorrelationIdDelegatingHandler>();

/*──────────────────────────────────────────────
  5. HttpClient  (Category API gibi dış çağrılar)
  ─────────────────────────────────────────────*/
builder.Services.AddHttpClient<CategoryApiClient>()
                .AddHttpMessageHandler<CorrelationIdDelegatingHandler>();

/*──────────────────────────────────────────────
  6. AutoMapper – Repository – Service
  ─────────────────────────────────────────────*/
builder.Services.AddAutoMapper(typeof(MappingProfile));
builder.Services.AddScoped<IVariationRepository, VariationRepository>();
builder.Services.AddScoped<IVariationService,    VariationService>();

/*──────────────────────────────────────────────
  7. Controllers / JSON / FluentValidation
  ─────────────────────────────────────────────*/
builder.Services.AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.ReferenceHandler       = ReferenceHandler.IgnoreCycles;
        o.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.AddFluentValidationAutoValidation()
                .AddFluentValidationClientsideAdapters();

// Validator’larınızı içeren assembly
builder.Services.AddValidatorsFromAssemblyContaining<CreateVariationDtoValidator>();

// Model-doğrulama hataları için özelleştirilmiş çıktı
builder.Services.Configure<ApiBehaviorOptions>(opt =>
{
    opt.InvalidModelStateResponseFactory = ctx =>
    {
        var errors = ctx.ModelState
                        .Where(e => e.Value!.Errors.Any())
                        .Select(e => new
                        {
                            Field    = e.Key,
                            Messages = e.Value!.Errors.Select(x => x.ErrorMessage)
                        });

        return new BadRequestObjectResult(new
        {
            message = "Model doğrulama hatası",
            errors
        });
    };
});

/*──────────────────────────────────────────────
  8. Swagger & CORS
  ─────────────────────────────────────────────*/
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title       = "Variation Microservice API",
        Version     = "v1",
        Description = "Variation management microservice"
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("MicroservicePolicy",
        p => p.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod());
});

/*──────────────────────────────────────────────
  9. Build & Middleware Pipeline
  ─────────────────────────────────────────────*/
var app = builder.Build();

// ❶ Correlation-Id middleware’i en başta
app.UseMiddleware<CorrelationIdMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Variation Microservice API V1"));
}

if (!app.Environment.IsDevelopment())
    app.UseHttpsRedirection();

app.UseCors("MicroservicePolicy");
app.UseSerilogRequestLogging();      // ❷ Serilog HTTP request log’u
app.UseAuthorization();
app.MapControllers();

/*──────────────────────────────────────────────
  10. Uygulama başlarken otomatik migration
  ─────────────────────────────────────────────*/
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<VariationDbContext>();
    db.Database.Migrate();
}

app.Run();