/*****************************************************
 *  Variation-Option Microservice – Program.cs (FULL)
 ****************************************************/
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;
using MongoDB.Driver;
using FluentValidation;
using FluentValidation.AspNetCore;

using VariationOptionMicroservice.Data;
using VariationOptionMicroservice.Data.Repositories;
using VariationOptionMicroservice.Service.Interfaces;
using VariationOptionMicroservice.Service.Services;
using VariationOptionMicroservice.Service.Mapping;
using VariationOptionMicroservice.Service.Validation;

using VariationOptionMicroservice.Http;        // CorrelationIdDelegatingHandler
using VariationOptionMicroservice.Middleware;  // CorrelationIdMiddleware

var builder = WebApplication.CreateBuilder(args);

/*──────────────────────────────────────────────
  1. Serilog
  ─────────────────────────────────────────────*/
const string serviceName = "VariationOptionMicroservice";
builder.Host.UseSerilog((ctx, lc) => lc
    .ReadFrom.Configuration(ctx.Configuration)   // appsettings*.json
    .Enrich.FromLogContext()
    .Enrich.WithProperty("ServiceName", serviceName)
    .WriteTo.Console());

/*──────────────────────────────────────────────
  2. MongoDB – log koleksiyonu
  ─────────────────────────────────────────────*/
builder.Services.AddSingleton<IMongoClient>(_ =>
    new MongoClient("mongodb://mongo:27017"));   // docker-compose’ta “mongo”

builder.Services.AddSingleton<IMongoDatabase>(sp =>
    sp.GetRequiredService<IMongoClient>()
      .GetDatabase("ECommerceLogs"));

// Eğer Address/Variation tarafındaki gibi özel log sınıfınız varsa
builder.Services.AddSingleton<VariationOptionActionLogger>();

/*──────────────────────────────────────────────
  3. Entity Framework – PostgreSQL
  ─────────────────────────────────────────────*/
builder.Services.AddDbContext<VariationOptionDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

/*──────────────────────────────────────────────
  4. Correlation-Id altyapısı
  ─────────────────────────────────────────────*/
builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<CorrelationIdDelegatingHandler>();

/*──────────────────────────────────────────────
  5. HttpClient  (Category / Variation servisleri)
  ─────────────────────────────────────────────*/
builder.Services.AddHttpClient<CategoryApiClient>()
                .AddHttpMessageHandler<CorrelationIdDelegatingHandler>();

/*──────────────────────────────────────────────
  6. AutoMapper – Repository – Service
  ─────────────────────────────────────────────*/
builder.Services.AddAutoMapper(typeof(MappingProfile));
builder.Services.AddScoped<IVariationOptionRepository, VariationOptionRepository>();
builder.Services.AddScoped<IVariationOptionService,    VariationOptionService>();

/*──────────────────────────────────────────────
  7. Controllers / JSON / FluentValidation
  ─────────────────────────────────────────────*/
builder.Services.AddControllers()
    .AddJsonOptions(opt =>
    {
        opt.JsonSerializerOptions.ReferenceHandler       = ReferenceHandler.IgnoreCycles;
        opt.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.AddFluentValidationAutoValidation()
                .AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssemblyContaining<CreateVariationOptionDtoValidator>();

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
        Title       = "Variation-Option Microservice API",
        Version     = "v1",
        Description = "Variation-Option management microservice"
    });

    // XML yorum dosyası eklemek isterseniz
    try
    {
        var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
        c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFile));
    }
    catch { /* XML yoksa hatayı yut */ }
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

app.UseMiddleware<CorrelationIdMiddleware>();   // ❶ Correlation-Id

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Variation-Option Microservice API V1"));
}

if (!app.Environment.IsDevelopment())
    app.UseHttpsRedirection();

app.UseCors("MicroservicePolicy");
app.UseSerilogRequestLogging();                 // ❷ HTTP log’ları
app.UseAuthorization();
app.MapControllers();


app.Run();