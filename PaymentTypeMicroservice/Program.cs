using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using PaymentTypeMicroservice.Data;
using PaymentTypeMicroservice.Data.Repositories;
using PaymentTypeMicroservice.Services.Interfaces;
using PaymentTypeMicroservice.Services;
using FluentValidation.AspNetCore;
using AutoMapper;
using PaymentTypeMicroservice.Mapping;
using Microsoft.Extensions.Configuration;

var builder = WebApplication.CreateBuilder(args);

// DbContext
builder.Services.AddDbContext<PaymentDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));


// AutoMapper
builder.Services.AddAutoMapper(typeof(PaymentTypeProfile));

// Services
builder.Services.AddScoped<IPaymentTypeService, PaymentTypeService>();
builder.Services.AddScoped<IPaymentTypeRepository, PaymentTypeRepository>();

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
