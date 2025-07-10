/*****************************************************
 *  ProductConfiguration Microservice – Program.cs
 ****************************************************/
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

using Serilog;
using MongoDB.Driver;


using ProductConfigurationMicroservice_Data;
using ProductConfigurationMicroservice_Data.Repositories;
using ProductConfigurationMicroservice_Service.Interfaces;
using ProductConfigurationMicroservice_Service.Services;


using ProductConfigurationMicroservice_Api.Http;        // CorrelationIdDelegatingHandler
using ProductConfigurationMicroservice_Api.Middleware;  // CorrelationIdMiddleware

var builder = WebApplication.CreateBuilder(args);

/*──────────────────────────────────────────────
  1. Serilog
  ─────────────────────────────────────────────*/
const string serviceName = "ProductConfigurationMicroservice";
builder.Host.UseSerilog((ctx, lc) => lc
    .ReadFrom.Configuration(ctx.Configuration)           // appsettings*.json
    .Enrich.FromLogContext()
    .Enrich.WithProperty("ServiceName", serviceName)
    .WriteTo.Console());

/*──────────────────────────────────────────────
  2. MongoDB – (HTTP + iş mantığı log’ları)
  ─────────────────────────────────────────────*/
builder.Services.AddSingleton<IMongoClient>(_ =>
    new MongoClient("mongodb://mongo:27017"));           // docker-compose’daki “mongo”

builder.Services.AddSingleton<IMongoDatabase>(sp =>
    sp.GetRequiredService<IMongoClient>()
      .GetDatabase("ECommerceLogs"));

builder.Services.AddSingleton<ProductConfigurationActionLogger>();

/*──────────────────────────────────────────────
  3. Correlation-Id altyapısı
  ─────────────────────────────────────────────*/
builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<CorrelationIdDelegatingHandler>();

builder.Services.AddHttpClient<ProductItemApiClient>()
                .AddHttpMessageHandler<CorrelationIdDelegatingHandler>();

builder.Services.AddHttpClient<VariationOptionApiClient>()
                .AddHttpMessageHandler<CorrelationIdDelegatingHandler>();

/*──────────────────────────────────────────────
  4. Entity Framework (Core) – PostgreSQL
  ─────────────────────────────────────────────*/
builder.Services.AddDbContext<ProductConfigurationDbContext>(opt =>
    opt.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly("ProductConfigurationMicroservice_Data")));

/*──────────────────────────────────────────────
  5. DI – Repositories & Services
  ─────────────────────────────────────────────*/
builder.Services.AddScoped<IProductConfigurationRepository, ProductConfigurationRepository>();
builder.Services.AddScoped<IProductConfigurationService,    ProductConfigurationService>();


/*──────────────────────────────────────────────
  7. Controllers / JSON
  ─────────────────────────────────────────────*/
builder.Services.AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.ReferenceHandler       = ReferenceHandler.IgnoreCycles;
        o.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

/*──────────────────────────────────────────────
  8. Swagger
  ─────────────────────────────────────────────*/
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(o =>
{
    o.EnableAnnotations();
    o.CustomSchemaIds(t => t.Name.Replace("Dto", ""));   // ProductConfigurationDto → ProductConfiguration
});

/*──────────────────────────────────────────────
  9. Build & Middleware Pipeline
  ─────────────────────────────────────────────*/
var app = builder.Build();

app.UseMiddleware<CorrelationIdMiddleware>();              // Correlation-Id

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsDevelopment())
    app.UseHttpsRedirection();

app.UseSerilogRequestLogging();                            // HTTP log’ları
app.MapControllers();

app.Run();