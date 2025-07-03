using Microsoft.EntityFrameworkCore;
using ProductCategoryMicroservice_Data;
using ProductCategoryMicroservice_Data.Repositories;
using ProductCategoryMicroservice_Service.Interfaces;
using ProductCategoryMicroservice_Service.Services;
using ProductCategoryMicroservice_Service.Mapping;
using ProductCategoryMicroservice_Service.Validation;          // FluentValidation’lar
using FluentValidation;
using FluentValidation.AspNetCore;
using Swashbuckle.AspNetCore.Annotations;

var builder = WebApplication.CreateBuilder(args);

/* -----------------------------------------------------------
 * ▶ 1. VERİTABANI
 * --------------------------------------------------------- */
builder.Services.AddDbContext<CategoryDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"), 
        b => b.MigrationsAssembly("ProductCategoryMicroservice_Data") // İşte bu satır!
    ));

/* -----------------------------------------------------------
 * ▶ 2. DEPENDENCY INJECTION
 * --------------------------------------------------------- */
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ICategoryService,    CategoryService>();

/* -----------------------------------------------------------
 * ▶ 3. AutoMapper & FluentValidation
 * --------------------------------------------------------- */
builder.Services.AddAutoMapper(typeof(CategoryProfile));

builder.Services.AddControllers();

// Yeni stil (v12) → Auto-validation + clientside adapters
builder.Services
        .AddFluentValidationAutoValidation()
        .AddFluentValidationClientsideAdapters();

// Tüm validator’ları bu assembly’den tara:
builder.Services.AddValidatorsFromAssemblyContaining<CreateCategoryDtoValidator>();

/* -----------------------------------------------------------
 * ▶ 4. Swagger (Annotations aktif)
 * --------------------------------------------------------- */
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => c.EnableAnnotations());

/* -----------------------------------------------------------
 * ▶ 5. Build & Middleware Pipeline
 * --------------------------------------------------------- */
var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.MapControllers();

/* Opsiyonel: otomatik migration */
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CategoryDbContext>();
    db.Database.Migrate();
}

app.Run();
