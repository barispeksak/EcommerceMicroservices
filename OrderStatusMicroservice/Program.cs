using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using OrderStatusMicroservice.Data;
using OrderStatusMicroservice.Data.Repositories;
using OrderStatusMicroservice.Services.Interfaces;
using OrderStatusMicroservice.Services;
using FluentValidation;
using OrderStatusMicroservice.Consumers;
using FluentValidation.AspNetCore;
using AutoMapper;
using OrderStatusMicroservice.Mapping;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using Serilog;
using OrderStatusMicroservice.Middleware;
using OrderStatusMicroservice.Http;
using OrderStatusMicroservice.Services.Logging;
using Serilog.AspNetCore;
using MassTransit;
using Microsoft.AspNetCore.ResponseCompression;

var builder = WebApplication.CreateBuilder(args);

// Add response compression early
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
});

// Configure DbContext
builder.Services.AddDbContext<OrderStatusDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        npgsqlOptions => npgsqlOptions.EnableRetryOnFailure(3)
                        .CommandTimeout(30)));

// HTTP Client
builder.Services.AddHttpClient("ShopOrderService", c =>
{
    c.BaseAddress = new Uri("http://shopordermicroservice:8080/");
});

// Controllers + Validation + JSON options - consolidated
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
builder.Services.AddAutoMapper(typeof(OrderStatusProfile));

// Services
builder.Services.AddScoped<IOrderStatusService, OrderStatusService>();
builder.Services.AddScoped<IOrderStatusRepository, OrderStatusRepository>();

// MassTransit & RabbitMQ
var config = builder.Configuration;
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<OrderCreatedConsumer>();
    x.AddConsumer<OrderFailedConsumer>();
    x.AddConsumer<StockNotAvailableConsumer>();

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
        
        // Configure prefetch count to avoid overloading
        busCfg.PrefetchCount = 10;
        
        // Configure retry policy
        busCfg.UseMessageRetry(r => r.Interval(3, 1000));
        
        busCfg.ConfigureEndpoints(ctx);
    });
});

// MongoDB for logging
builder.Services.AddSingleton<IMongoClient>(sp =>
    new MongoClient(builder.Configuration.GetConnectionString("MongoDb") ?? "mongodb://mongo:27017"));

builder.Services.AddSingleton<IMongoDatabase>(sp =>
    sp.GetRequiredService<IMongoClient>().GetDatabase("ECommerceLogs"));

builder.Services.AddSingleton<OrderStatusActionLogger>();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Serilog
const string serviceName = "OrderStatusMicroservice";
builder.Host.UseSerilog((ctx, svc, cfg) => cfg
    .ReadFrom.Configuration(ctx.Configuration)
    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("System", Serilog.Events.LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("ServiceName", serviceName)
    .WriteTo.Console());

// Build the app
var app = builder.Build();

// Configure middleware pipeline in correct order
app.UseResponseCompression();
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseRouting();
app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Docker"))
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Apply migrations if configured
if (app.Environment.IsDevelopment() || 
    Environment.GetEnvironmentVariable("APPLY_MIGRATIONS") == "true")
{
    try
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<OrderStatusDbContext>();
        dbContext.Database.Migrate();
        app.Logger.LogInformation("Database migrations applied successfully");
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "An error occurred while applying migrations");
    }
}

app.Run();