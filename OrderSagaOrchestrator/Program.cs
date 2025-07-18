using MassTransit;
using MassTransit.EntityFrameworkCoreIntegration;
using Microsoft.EntityFrameworkCore;
using OrderSagaOrchestrator.Data;
using OrderSagaOrchestrator;
using Microsoft.OpenApi.Models;
using System;

var builder = WebApplication.CreateBuilder(args);
var cfg = builder.Configuration;

// Get connection string from configuration
var connectionString = cfg.GetConnectionString("DefaultConnection");
Console.WriteLine($"Using connection string: {connectionString}");

// Register DB context (still needed for other parts of the application)
builder.Services.AddDbContext<OrderSagaDbContext>(options =>
    options.UseNpgsql(connectionString));

// Configure MassTransit with EntityFramework repository (NO OUTBOX)
builder.Services.AddMassTransit(x =>
{
    // Configure the state machine saga
    x.AddSagaStateMachine<OrderStateMachine, SagaState>()
     .EntityFrameworkRepository(r =>
     {
         r.ConcurrencyMode = ConcurrencyMode.Optimistic; // Use optimistic concurrency with Version
         r.ExistingDbContext<OrderSagaDbContext>();
         r.UsePostgres();
     });

    // Configure RabbitMQ transport
    x.UsingRabbitMq((context, cfg) =>
    {
        var rmq = builder.Configuration.GetSection("RabbitMQ");
        
        // Configure host
        cfg.Host(rmq["Host"], "/", h =>
        {
            h.Username(rmq["Username"] ?? "guest");
            h.Password(rmq["Password"] ?? "guest");
        });

        // Configure scheduler support for timeout handling
        cfg.UseDelayedRedelivery(r => r.Intervals(TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(15), TimeSpan.FromMinutes(30)));
        cfg.UseMessageRetry(r => r.Immediate(5));

        // Configure endpoints
        cfg.ConfigureEndpoints(context);
    });
});

// Add controllers
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "OrderSagaOrchestrator API", Version = "v1" });
});

// Build the application
var app = builder.Build();

// Initialize database
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<OrderSagaDbContext>();
    
    // Create the database if it doesn't exist
    dbContext.Database.EnsureCreated();
    Console.WriteLine("Database initialized successfully.");
}

if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Docker") || true)
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "OrderSagaOrchestrator v1"));
}

// Configure the application
app.MapControllers();

// Start the application
app.Run();