using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using ShippingTypeMicroservice.Data;
using ShippingTypeMicroservice.Data.Repositories;
using ShippingTypeMicroservice.Services.Interfaces;
using ShippingTypeMicroservice.Services;
using FluentValidation.AspNetCore;
using AutoMapper;
using ShippingTypeMicroservice.Mapping;
using Microsoft.Extensions.Configuration;

var builder = WebApplication.CreateBuilder(args);

// DbContext
builder.Services.AddDbContext<ShippingDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));


// AutoMapper
builder.Services.AddAutoMapper(typeof(ShippingTypeProfile));

// Services
builder.Services.AddScoped<IShippingTypeService, ShippingTypeService>();
builder.Services.AddScoped<IShippingTypeRepository, ShippingTypeRepository>();

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
