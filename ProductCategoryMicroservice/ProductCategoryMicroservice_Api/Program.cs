using Microsoft.EntityFrameworkCore;
using ProductCategoryMicroservice_Data;
using ProductCategoryMicroservice_Data.Repositories;
using ProductCategoryMicroservice_Service.Interfaces;
using ProductCategoryMicroservice_Service.Services;
using ProductCategoryMicroservice_Service.Mapping;
using ProductCategoryMicroservice_Service.Validation;

using FluentValidation;
using FluentValidation.AspNetCore;

using Serilog;
using MongoDB.Driver;

using ProductCategoryMicroservice_Api.Middleware;
using ProductCategoryMicroservice_Api.Http;           // CorrelationIdDelegatingHandler
using Swashbuckle.AspNetCore.Annotations;

var builder = WebApplication.CreateBuilder(args);

/*──────────────────────────────────────────────
  1. Serilog
  ─────────────────────────────────────────────*/
const string serviceName = "ProductCategoryMicroservice";
builder.Host.UseSerilog((ctx, lc) => lc
    .ReadFrom.Configuration(ctx.Configuration)       // appsettings.json
    .Enrich.FromLogContext()
    .Enrich.WithProperty("ServiceName", serviceName)
    .WriteTo.Console());

/*──────────────────────────────────────────────
  2. MongoDB – (HTTP ve iş mantığı log’ları)
  ─────────────────────────────────────────────*/
builder.Services.AddSingleton<IMongoClient>(_ =>
    new MongoClient("mongodb://mongo:27017"));           // docker-compose'da container adı "mongo"

builder.Services.AddSingleton<IMongoDatabase>(sp =>
    sp.GetRequiredService<IMongoClient>()
      .GetDatabase("ECommerceLogs"));                    // aynı veritabanı

builder.Services.AddSingleton<ProductCategoryActionLogger>();;   // ← eğer böyle bir sınıfınız varsa

/*──────────────────────────────────────────────
  3. Correlation-Id Altyapısı
  ─────────────────────────────────────────────*/
builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<CorrelationIdDelegatingHandler>();

// Örnek outbound HttpClient (kullanıyorsanız)
// builder.Services.AddHttpClient("SomeOtherService", c =>
// {
//     c.BaseAddress = new Uri("http://otherservice");
// })
// .AddHttpMessageHandler<CorrelationIdDelegatingHandler>();

/*──────────────────────────────────────────────
  4. Entity Framework
  ─────────────────────────────────────────────*/
builder.Services.AddDbContext<CategoryDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly("ProductCategoryMicroservice_Data")));

/*──────────────────────────────────────────────
  5. DI – Repositories & Services
  ─────────────────────────────────────────────*/
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ICategoryService,    CategoryService>();

/*──────────────────────────────────────────────
  6. AutoMapper & FluentValidation
  ─────────────────────────────────────────────*/
builder.Services.AddAutoMapper(typeof(CategoryProfile));

builder.Services.AddControllers();

builder.Services
        .AddFluentValidationAutoValidation()
        .AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssemblyContaining<CreateCategoryDtoValidator>();

/*──────────────────────────────────────────────
  7. Swagger
  ─────────────────────────────────────────────*/
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => c.EnableAnnotations());

/*──────────────────────────────────────────────
  8. Build & Middleware Pipeline
  ─────────────────────────────────────────────*/
var app = builder.Build();

app.UseMiddleware<CorrelationIdMiddleware>();      // ilk middleware – CorrelationId

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsDevelopment())
    app.UseHttpsRedirection();

app.UseSerilogRequestLogging();                    // HTTP log’ları
app.MapControllers();



app.Run();