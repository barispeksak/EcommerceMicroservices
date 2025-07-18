// ───────────── USING BLOĞU ─────────────
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using Serilog;
using FluentValidation;
using FluentValidation.AspNetCore;
using MongoDB.Driver;
using MassTransit;

using ShoppingCartMicroservice_Service.Interfaces;
using ShoppingCartMicroservice_Service.Services;
using ShoppingCartMicroservice_Service.Validation;
using ShoppingCartMicroservice_Api.Middleware;          // CorrelationIdMiddleware
using ShoppingCartMicroservice_Api.Http;                // CorrelationIdDelegatingHandler
using ShoppingCartMicroservice_Api.Consumers; 
using ShoppingCartMicroservice_Api.Storage;          // <-- NEW
using ShoppingCartMicroservice_Service;                 // MongoDbSettings, ActionLogger
                                                        // ProductClient, ProductItemClient (varsa)

// ───────────── HOST & SERILOG ─────────────
var builder = WebApplication.CreateBuilder(args);
var cfg     = builder.Configuration;  

const string serviceName = "ShoppingCartMicroservice";
builder.Host.UseSerilog((ctx, lc) => lc
    .ReadFrom.Configuration(ctx.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("ServiceName", serviceName)
    .WriteTo.Console());

// ───────────── MongoDB (log) ─────────────
builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection("MongoDb"));

builder.Services.AddSingleton<IMongoClient>(_ =>
    new MongoClient(builder.Configuration["MongoDb:ConnectionString"]));

builder.Services.AddSingleton<ShoppingCartActionLogger>();

// ───────────── RabbitMQ & MassTransit ─────────────
           // kenara al

builder.Services.AddSingleton<IStockRepository, RedisStockRepository>();

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<FinalStockCheckRequestedConsumer>(); // For orchestration pattern
    x.AddConsumer<RollbackStockConsumer>();
    //x.AddConsumer<ReserveStockConsumer>(); // Not needed for orchestration

    x.UsingRabbitMq((ctx, bus) =>
    {
        var rmq = cfg.GetSection("RabbitMQ");
        bus.Host(rmq["Host"] ?? "rabbitmq", "/", h =>
        {
            h.Username(rmq["Username"] ?? "guest");
            h.Password(rmq["Password"] ?? "guest");
        });

        // Configure endpoint for FinalStockCheckRequested
        bus.ReceiveEndpoint("FinalStockCheckRequested", e =>
        {
            e.ConfigureConsumer<FinalStockCheckRequestedConsumer>(ctx);
        });

        bus.ConfigureEndpoints(ctx);
    });
});

// ───────────── Redis (cache) ─────────────
builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
    ConnectionMultiplexer.Connect(cfg.GetConnectionString("Redis")));

builder.Services.AddStackExchangeRedisCache(opt =>
{
    opt.Configuration = cfg.GetConnectionString("Redis");
    opt.InstanceName  = "ecom-cart:";
});

// ───────────── Correlation-Id ─────────────
builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<CorrelationIdDelegatingHandler>();

// ───────────── HttpClients ─────────────
builder.Services.AddHttpClient<ProductItemClient>(c =>
        c.BaseAddress = new Uri(cfg["ServiceUrls:ProductItem"]))
    .AddHttpMessageHandler<CorrelationIdDelegatingHandler>();

builder.Services.AddHttpClient<ProductClient>(c =>
        c.BaseAddress = new Uri(cfg["ServiceUrls:Product"]))
    .AddHttpMessageHandler<CorrelationIdDelegatingHandler>();

// ───────────── DI + Validation ─────────────
builder.Services.AddScoped<IShoppingCartService, ShoppingCartService>();

builder.Services.AddControllers();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssemblyContaining<CreateShoppingCartDtoValidator>();

// ───────────── Swagger ─────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ───────────── PIPELINE ─────────────
var app = builder.Build();

app.UseMiddleware<CorrelationIdMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.Run();