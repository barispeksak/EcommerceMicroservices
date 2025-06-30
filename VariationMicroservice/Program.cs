using Microsoft.EntityFrameworkCore;
using FluentValidation;
using FluentValidation.AspNetCore;
using System.Reflection;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using VariationMicroservice.Data;
using VariationMicroservice.Data.Repositories;
using VariationMicroservice.Service.Interfaces;
using VariationMicroservice.Service.Services;
using VariationMicroservice.Service.Mapping;
 // ✅ CategoryApiClient için ekledik

var builder = WebApplication.CreateBuilder(args);

// 🔁 PostgreSQL bağlantı
builder.Services.AddDbContext<VariationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// 🔁 HttpClient (Category API için)
builder.Services.AddHttpClient<CategoryApiClient>();

// 🔁 AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

// 🔁 FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// 🔁 Repository
builder.Services.AddScoped<IVariationRepository, VariationRepository>();

// 🔁 Service
builder.Services.AddScoped<IVariationService, VariationService>();

// 🔁 Controllers
builder.Services.AddControllers()
    .AddJsonOptions(opt =>
    {
        opt.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        opt.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

// 🔁 Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Variation Microservice API",
        Version = "v1",
        Description = "Variation management microservice"
    });
});

// 🔁 FluentValidation özelleştirilmiş hata çıktısı
builder.Services.Configure<ApiBehaviorOptions>(opt =>
{
    opt.InvalidModelStateResponseFactory = ctx =>
    {
        var errors = ctx.ModelState
            .Where(e => e.Value!.Errors.Any())
            .Select(e => new
            {
                Field = e.Key,
                Messages = e.Value!.Errors.Select(x => x.ErrorMessage)
            });

        return new BadRequestObjectResult(new
        {
            message = "Model doğrulama hatası",
            errors
        });
    };
});

var app = builder.Build();

// 🔁 Geliştirme ortamı için Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Variation Microservice API V1");
    });
}

// app.UseHttpsRedirection(); // HTTPS yönlendirmesini kaldırdık (geliştirme ortamı için mantıklı)

app.UseAuthorization();

app.MapControllers();

app.Run();
