/*****************************************************
 *  ProductItem Microservice – Program.cs  (FULL)
 ****************************************************/
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

using Serilog;
using MongoDB.Driver;

using FluentValidation;
using FluentValidation.AspNetCore;

using ProductItemMicroservice_Data;
using ProductItemMicroservice_Data.Repositories;
using ProductItemMicroservice_Service.Interfaces;
using ProductItemMicroservice_Service.Services;
using ProductItemMicroservice_Service.Mapping;
using ProductItemMicroservice_Service.Validation;

using ProductItemMicroservice_Api.Http;        // CorrelationIdDelegatingHandler
using ProductItemMicroservice_Api.Middleware;  // CorrelationIdMiddleware

var builder = WebApplication.CreateBuilder(args);

/*──────────────────────────────────────────────
  1. Serilog
  ─────────────────────────────────────────────*/
const string serviceName = "ProductItemMicroservice";
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

builder.Services.AddSingleton<ProductItemActionLogger>();   // kendi log sınıfınız

/*──────────────────────────────────────────────
  3. Correlation-Id altyapısı
  ─────────────────────────────────────────────*/
builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<CorrelationIdDelegatingHandler>();

builder.Services.AddHttpClient<ProductApiClient>()          // başka servise giden çağrılar
                .AddHttpMessageHandler<CorrelationIdDelegatingHandler>();

/*──────────────────────────────────────────────
  4. Entity Framework (Core) – PostgreSQL
  ─────────────────────────────────────────────*/
builder.Services.AddDbContext<ProductItemDbContext>(opt =>
    opt.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly("ProductItemMicroservice_Data")));

/*──────────────────────────────────────────────
  5. DI – Repositories & Services
  ─────────────────────────────────────────────*/
builder.Services.AddScoped<IProductItemRepository, ProductItemRepository>();
builder.Services.AddScoped<IProductItemService,    ProductItemService>();

/*──────────────────────────────────────────────
  6. AutoMapper & FluentValidation
  ─────────────────────────────────────────────*/
builder.Services.AddAutoMapper(typeof(ProductItemProfile));

builder.Services
        .AddFluentValidationAutoValidation()
        .AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssemblyContaining<CreateProductItemDtoValidator>();

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
    o.CustomSchemaIds(t => t.Name.Replace("Dto", ""));   // ProductItemDto → ProductItem
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