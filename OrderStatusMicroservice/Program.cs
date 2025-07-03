using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using OrderStatusMicroservice.Data;
using OrderStatusMicroservice.Data.Repositories;
using OrderStatusMicroservice.Services.Interfaces;
using OrderStatusMicroservice.Services;
using FluentValidation.AspNetCore;
using AutoMapper;
using OrderStatusMicroservice.Mapping;
using Microsoft.Extensions.Configuration;

var builder = WebApplication.CreateBuilder(args);

// DbContext
builder.Services.AddDbContext<OrderStatusDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHttpClient("ShopOrderService", c =>
{
    c.BaseAddress = new Uri("http://shopordermicroservice:8080/"); // Docker container adı + port
});




// AutoMapper
builder.Services.AddAutoMapper(typeof(OrderStatusProfile));

// Services
builder.Services.AddScoped<IOrderStatusService, OrderStatusService>();
builder.Services.AddScoped<IOrderStatusRepository, OrderStatusRepository>();

// FluentValidation
builder.Services.AddControllers()
    .AddFluentValidation(cfg => cfg.RegisterValidatorsFromAssemblyContaining<Program>());

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();
app.Run();
