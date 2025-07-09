/*****************************************************
 *  Product Microservice – Program.cs (FULL)
 ****************************************************/
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Serilog;
using MongoDB.Driver;

using FluentValidation;
using FluentValidation.AspNetCore;

using ProductMicroservice_Data;
using ProductMicroservice_Data.Repositories;
using ProductMicroservice_Service.Interfaces;
using ProductMicroservice_Service.Services;
using ProductMicroservice_Service.Mapping;
using ProductMicroservice_Service.Validation;

using ProductMicroservice_Api.Http;        // CorrelationIdDelegatingHandler
using ProductMicroservice_Api.Middleware;  // CorrelationIdMiddleware

var builder = WebApplication.CreateBuilder(args);

/*──────────────────────────────────────────────
  1. Serilog
  ─────────────────────────────────────────────*/
const string serviceName = "ProductMicroservice";
builder.Host.UseSerilog((ctx, lc) => lc
    .ReadFrom.Configuration(ctx.Configuration)           // appsettings*.json
    .Enrich.FromLogContext()
    .Enrich.WithProperty("ServiceName", serviceName)
    .WriteTo.Console());

/*──────────────────────────────────────────────
  2. MongoDB – (HTTP + iş mantığı log’ları)
  ─────────────────────────────────────────────*/
builder.Services.AddSingleton<IMongoClient>(_ =>
    new MongoClient("mongodb://mongo:27017"));           // docker-compose’da “mongo”

builder.Services.AddSingleton<IMongoDatabase>(sp =>
    sp.GetRequiredService<IMongoClient>()
      .GetDatabase("ECommerceLogs"));

builder.Services.AddSingleton<ProductActionLogger>();    // kendi log sınıfınız

/*──────────────────────────────────────────────
  3. Correlation-Id altyapısı
  ─────────────────────────────────────────────*/
builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<CorrelationIdDelegatingHandler>();

/* Variation / Category servislerine giden isteklerde Id taşımak için örnek */
builder.Services.AddHttpClient<CategoryApiClient>()
                .AddHttpMessageHandler<CorrelationIdDelegatingHandler>();

/*──────────────────────────────────────────────
  4. Entity Framework (Core) – PostgreSQL
  ─────────────────────────────────────────────*/
builder.Services.AddDbContext<ProductDbContext>(opt =>
    opt.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly("ProductMicroservice_Data")));

/*──────────────────────────────────────────────
  5. DI – Repositories & Services
  ─────────────────────────────────────────────*/
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductService,    ProductService>();

/*──────────────────────────────────────────────
  6. AutoMapper & FluentValidation
  ─────────────────────────────────────────────*/
builder.Services.AddAutoMapper(typeof(ProductProfile));

builder.Services
        .AddFluentValidationAutoValidation()
        .AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssemblyContaining<CreateProductDtoValidator>();

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
    o.CustomSchemaIds(t => t.Name.Replace("Dto", ""));   // ProductDto → Product
});

/*──────────────────────────────────────────────
  9. Build & Middleware Pipeline
  ─────────────────────────────────────────────*/
var app = builder.Build();

app.UseMiddleware<CorrelationIdMiddleware>();            // Correlation-Id

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsDevelopment())
    app.UseHttpsRedirection();

app.UseSerilogRequestLogging();                          // HTTP log’ları
app.MapControllers();



app.Run();