using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using ProductConfigurationMicroservice_Data;
using ProductConfigurationMicroservice_Data.Repositories;
using ProductConfigurationMicroservice_Service.Mapping;
using ProductConfigurationMicroservice_Service.Services;
using ProductConfigurationMicroservice_Service.Interfaces;
using Swashbuckle.AspNetCore.Annotations;

var builder = WebApplication.CreateBuilder(args);

/* ---------- typed HttpClient’lar ---------- */
builder.Services.AddHttpClient<ProductItemApiClient>();      
builder.Services.AddHttpClient<VariationOptionApiClient>();  


/* ---------- DbContext ---------- */
builder.Services.AddDbContext<ProductConfigurationDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));  

/* ---------- DI bağlamaları ---------- */
builder.Services.AddScoped<IProductConfigurationRepository, ProductConfigurationRepository>();
builder.Services.AddScoped<IProductConfigurationService,    ProductConfigurationService>();

/* ---------- AutoMapper ---------- */
builder.Services.AddAutoMapper(typeof(ProductConfigurationProfile).Assembly);

/* ---------- Swagger ---------- */
builder.Services.AddSwaggerGen(o =>
{
    o.EnableAnnotations();
    o.CustomSchemaIds(t => t.Name.Replace("Dto", ""));   // ProductConfigurationDto → ProductConfiguration
});

/* ---------- MVC ---------- */
builder.Services.AddControllers();

var app = builder.Build();

/* ---------- HTTP pipeline ---------- */
app.UseHttpsRedirection();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();
app.Run();
