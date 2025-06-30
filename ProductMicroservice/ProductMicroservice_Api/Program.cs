using Microsoft.EntityFrameworkCore;
using FluentValidation;
using Swashbuckle.AspNetCore.Annotations;
using ProductMicroservice.Data;
using ProductMicroservice.Data.Repositories;
using ProductMicroservice.Service.Mapping;
using ProductMicroservice.Service.Services;
using ProductMicroservice.Service.Validation;
using ProductMicroservice.Service.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient<CategoryApiClient>();

/* ---------- DbContext ---------- */
builder.Services.AddDbContext<ProductDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

/* ---------- DI bağlamaları ---------- */
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductService,    ProductService>();

/* ---------- AutoMapper & FluentValidation ---------- */
builder.Services.AddAutoMapper(typeof(ProductProfile).Assembly);
builder.Services.AddValidatorsFromAssemblyContaining<CreateProductDtoValidator>();

/* ---------- Swagger ---------- */
builder.Services.AddSwaggerGen(o =>
{
    o.EnableAnnotations();
    o.CustomSchemaIds(t => t.Name.Replace("Dto", ""));   // ProductDto → Product
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
