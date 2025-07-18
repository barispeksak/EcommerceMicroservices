using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using PaymentTypeMicroservice.Data;
using PaymentTypeMicroservice.Data.Repositories;
using PaymentTypeMicroservice.Services.Interfaces;
using PaymentTypeMicroservice.Services;
using FluentValidation;
using FluentValidation.AspNetCore;
using AutoMapper;
using PaymentTypeMicroservice.Mapping;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver; // * 
using Serilog; // * 
using PaymentTypeMicroservice.Middleware; // * 
using PaymentTypeMicroservice.Http; // * 
using PaymentTypeMicroservice.Services.Logging; // * 
using Serilog.AspNetCore;
using MassTransit;
using PaymentTypeMicroservice.Consumers;
using System;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

// DbContext
builder.Services.AddDbContext<PaymentDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Controllers + Validation
builder.Services.AddControllers()
    .AddJsonOptions(options => {
        options.JsonSerializerOptions.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
    })
    .AddFluentValidation(config => {
        config.RegisterValidatorsFromAssemblyContaining<Program>();
    });

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssemblyContaining(typeof(Program));

// AutoMapper
builder.Services.AddAutoMapper(typeof(PaymentTypeProfile));

var config   = builder.Configuration;      // ① DI dışı yedek referans

builder.Services.AddMassTransit(x =>
{
    // Add the payment consumer
    x.AddConsumer<ProcessPaymentRequestedConsumer>();
    
    x.UsingRabbitMq((ctx, busCfg) =>
    {
        var rmq = config.GetSection("RabbitMQ");
        string host = rmq["Host"] ?? "rabbitmq";
        string username = rmq["Username"] ?? "guest";  
        string password = rmq["Password"] ?? "guest";
        
        busCfg.Host(host, "/", h =>
        {
            h.Username(username);
            h.Password(password);
        });
        
        busCfg.PrefetchCount = 10;
        busCfg.UseMessageRetry(r => r.Interval(3, 1000));
        
        busCfg.ConfigureEndpoints(ctx);
    });
});
// Services
builder.Services.AddScoped<IPaymentTypeService, PaymentTypeService>();
builder.Services.AddScoped<IPaymentTypeRepository, PaymentTypeRepository>();

// FluentValidation
builder.Services.AddControllers()
    .AddFluentValidation(cfg => cfg.RegisterValidatorsFromAssemblyContaining<Program>());

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IMongoClient>(sp =>
    new MongoClient("mongodb://mongo:27017")); // veya "localhost" – Docker ortamına göre değişir

builder.Services.AddSingleton<IMongoDatabase>(sp =>
    sp.GetRequiredService<IMongoClient>().GetDatabase("ECommerceLogs"));

builder.Services.AddSingleton<PaymentTypeActionLogger>();

// Serilog
const string serviceName = "PaymentTypeTypeMicroservice"; // *
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

// Apply migrations on startup - matching the same approach as ShopOrderMicroservice
using (var scope = app.Services.CreateScope())
{
    if (app.Environment.IsDevelopment() || Environment.GetEnvironmentVariable("APPLY_MIGRATIONS") == "true")
    {
        try
        {
            var paymentDbContext = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();
            paymentDbContext.Database.Migrate();
            
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("Database migrations applied successfully");
        }
        catch (Exception ex)
        {
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "An error occurred while applying migrations");
        }
    }
}

app.Run();
