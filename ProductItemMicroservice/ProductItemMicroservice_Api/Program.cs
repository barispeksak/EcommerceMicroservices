using Microsoft.EntityFrameworkCore;
using FluentValidation;
using Swashbuckle.AspNetCore.Annotations;

using ProductItemMicroservice_Data;
using ProductItemMicroservice_Data.Repositories;

using ProductItemMicroservice_Service.Mapping;
using ProductItemMicroservice_Service.Services;
using ProductItemMicroservice_Service.Validation;
using ProductItemMicroservice_Service.Interfaces;

var builder = WebApplication.CreateBuilder(args);

/* ---------- HTTP client (typed) ---------- */
builder.Services.AddHttpClient<ProductApiClient>();   // tıpkı CategoryApiClient gibi

/* ---------- DbContext ---------- */
builder.Services.AddDbContext<ProductItemDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

/* ---------- DI ---------- */
builder.Services.AddScoped<IProductItemRepository, ProductItemRepository>();
builder.Services.AddScoped<IProductItemService,    ProductItemService>();

/* ---------- AutoMapper & FluentValidation ---------- */
builder.Services.AddAutoMapper(typeof(ProductItemProfile).Assembly);
builder.Services.AddValidatorsFromAssemblyContaining<CreateProductItemDtoValidator>();

/* ---------- Swagger ---------- */
builder.Services.AddSwaggerGen(o =>
{
    o.EnableAnnotations();
    o.CustomSchemaIds(t => t.Name.Replace("Dto", "")); // ProductItemDto → ProductItem
});

/* ---------- MVC ---------- */
builder.Services.AddControllers();

var app = builder.Build();

/* ---------- Pipeline ---------- */
app.UseHttpsRedirection();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();
app.Run();
