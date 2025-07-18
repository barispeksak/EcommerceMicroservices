/*****************************************************
 *  ProductItem Microservice – Program.cs  (FULL)
 ****************************************************/
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

using Serilog;
using MongoDB.Driver;
using MassTransit;

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

builder.Services.AddSingleton<ProductItemActionLogger>(); 

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

// Initialize the database with migrations
try
{
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ProductItemDbContext>();
        dbContext.Database.Migrate();
        
        // Seed test data if needed
        if (!dbContext.ProductItems.Any())
        {
            dbContext.ProductItems.AddRange(
                new ProductItemMicroservice_Data.Entities.ProductItem 
                { 
                    Sku = "PROD-001", 
                    QuantityInStock = 10, 
                    Price = 100m, 
                    Currency = "TRY", 
                    ProductId = 1 
                },
                new ProductItemMicroservice_Data.Entities.ProductItem 
                { 
                    Sku = "PROD-002", 
                    QuantityInStock = 20, 
                    Price = 150m, 
                    Currency = "TRY", 
                    ProductId = 2 
                },
                new ProductItemMicroservice_Data.Entities.ProductItem 
                { 
                    Sku = "PROD-003", 
                    QuantityInStock = 30, 
                    Price = 200m, 
                    Currency = "TRY", 
                    ProductId = 3 
                }
            );
            dbContext.SaveChanges();
        }
    }
    Log.Information("Database initialized successfully with migrations");
}
catch (Exception ex)
{
    Log.Error(ex, "An error occurred while initializing the database with migrations");
}

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