using Microsoft.EntityFrameworkCore;
using ShoppingCartMicroservice_Data;
using ShoppingCartMicroservice_Service.Interfaces;
using ShoppingCartMicroservice_Data.Repositories;
using ShoppingCartMicroservice_Service.Services;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Builder;
using AutoMapper;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddHttpClient<ProductItemApiClient>();     

// DbContext
builder.Services.AddDbContext<ShoppingCartDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));  

// Repository & Service
builder.Services.AddScoped<IShoppingCartRepository, ShoppingCartRepository>();
builder.Services.AddScoped<IShoppingCartService, ShoppingCartService>();

// AutoMapper
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// FluentValidation
builder.Services.AddControllers()
    .AddFluentValidation(config => 
        config.RegisterValidatorsFromAssemblyContaining<Program>());

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.EnableAnnotations();
});

var app = builder.Build();

// Configure the HTTP request pipeline.

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
