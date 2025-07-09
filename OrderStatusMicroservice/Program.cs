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
using MongoDB.Driver; // * 
using Serilog; // * 
using OrderStatusMicroservice.Middleware; // * 
using OrderStatusMicroservice.Http; // * 
using OrderStatusMicroservice.Services.Logging; // * 
using Serilog.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// DbContext
builder.Services.AddDbContext<OrderStatusDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHttpClient("ShopOrderService", c =>
{
    c.BaseAddress = new Uri("http://shopordermicroservice:8080/"); // Docker container adı + port
});

// Controllers + Validation
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
    }) // *
    .AddFluentValidation(cfg => cfg.RegisterValidatorsFromAssemblyContaining<Program>());


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

builder.Services.AddSingleton<IMongoClient>(sp =>
    new MongoClient("mongodb://mongo:27017")); // veya "localhost" – Docker ortamına göre değişir

builder.Services.AddSingleton<IMongoDatabase>(sp =>
    sp.GetRequiredService<IMongoClient>().GetDatabase("ECommerceLogs"));

builder.Services.AddSingleton<OrderStatusActionLogger>();

// Serilog
const string serviceName = "OrderStatusTypeMicroservice"; // *
builder.Host.UseSerilog((ctx, svc, cfg) => cfg // *
    .ReadFrom.Configuration(ctx.Configuration) // *
    .Enrich.FromLogContext() // *
    .Enrich.WithProperty("ServiceName", serviceName) // *
    .WriteTo.Console()); // *

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();


app.UseMiddleware<CorrelationIdMiddleware>(); // *
app.UseRouting(); // *
app.UseSerilogRequestLogging(); // *

app.UseHttpsRedirection();
app.MapControllers();
app.Run();
