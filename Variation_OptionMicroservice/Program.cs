using Microsoft.EntityFrameworkCore;
using FluentValidation;
using FluentValidation.AspNetCore;
using System.Reflection;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Variation_OptionMicroservice.Data;
using Variation_OptionMicroservice.Data.Repositories;
using Variation_OptionMicroservice.Service.Interfaces;
using Variation_OptionMicroservice.Service.Services;
using Variation_OptionMicroservice.Service.Mapping;

var builder = WebApplication.CreateBuilder(args);

// 🔁 PostgreSQL bağlantı
builder.Services.AddDbContext<Variation_OptionDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

// FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// Repository
builder.Services.AddScoped<IVariationOptionRepository, VariationOptionRepository>();

// Service
builder.Services.AddScoped<IVariationOptionService, VariationOptionService>();

// Controllers
builder.Services.AddControllers()
    .AddJsonOptions(opt =>
    {
        opt.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        opt.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Variation Option Microservice API",
        Version = "v1",
        Description = "Variation Option management microservice"
    });
    
    // XML comments için
    try
    {
        var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
    }
    catch (Exception)
    {
        // XML dosyası yoksa hata verme
    }
});

// API Behavior Options
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

// Seed data için
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<Variation_OptionDbContext>();
    await context.Database.MigrateAsync();
    
    // Test verisi ekle
    if (!context.Variations.Any())
    {
        var variations = new[]
        {
            new Variation_OptionMicroservice.Data.Entities.Variation { VarTypeName = "Renk", CategoryId = 1 },
            new Variation_OptionMicroservice.Data.Entities.Variation { VarTypeName = "Beden", CategoryId = 1 },
            new Variation_OptionMicroservice.Data.Entities.Variation { VarTypeName = "Numara", CategoryId = 2 }
        };
        
        context.Variations.AddRange(variations);
        await context.SaveChangesAsync();
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Variation Option Microservice API V1");
    });
}

app.UseAuthorization();
app.MapControllers();

app.Run();