using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using Serilog;
using ShoppingCartMicroservice_Service.Interfaces;
using ShoppingCartMicroservice_Service.Services;
using ShoppingCartMicroservice_Service.Validation;
using FluentValidation;
using FluentValidation.AspNetCore;
using ShoppingCartMicroservice_Api.Middleware;   // CorrelationIdMiddleware
using ShoppingCartMicroservice_Api.Http;         // CorrelationIdDelegatingHandler
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

/*──────────────────── Serilog ────────────────────*/
const string serviceName = "ShoppingCartMicroservice";
builder.Host.UseSerilog((ctx, lc) => lc
    .ReadFrom.Configuration(ctx.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("ServiceName", serviceName)
    .WriteTo.Console());

/*──────────────────── MongoDB (log) ───────────────*/
builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection("MongoDb"));

builder.Services.AddSingleton<IMongoClient>(_ =>
    new MongoClient(builder.Configuration["MongoDb:ConnectionString"]));

builder.Services.AddSingleton<ShoppingCartActionLogger>();

/*──────────────────── Redis (cache) ───────────────*/
builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
    ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis")));

builder.Services.AddStackExchangeRedisCache(opt =>
{
    opt.Configuration = builder.Configuration.GetConnectionString("Redis");
    opt.InstanceName  = "ecom-cart:";
});

/*──────────────────── Correlation-Id ──────────────*/
builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<CorrelationIdDelegatingHandler>();

/*──────────────────── HttpClients ────────────────*/
builder.Services.AddHttpClient<ProductItemClient>(c =>
    c.BaseAddress = new Uri(builder.Configuration["ServiceUrls:ProductItem"]))
    .AddHttpMessageHandler<CorrelationIdDelegatingHandler>();

builder.Services.AddHttpClient<ProductClient>(c =>
    c.BaseAddress = new Uri(builder.Configuration["ServiceUrls:Product"]))
    .AddHttpMessageHandler<CorrelationIdDelegatingHandler>();

/*──────────────────── DI + Validation ────────────*/
builder.Services.AddScoped<IShoppingCartService, ShoppingCartService>();

builder.Services.AddControllers();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssemblyContaining<CreateShoppingCartDtoValidator>();

/*──────────────────── Swagger ─────────────────────*/
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

/*──────────────────── Build & Pipeline ───────────*/
var app = builder.Build();

app.UseMiddleware<CorrelationIdMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();   // Auth görevini gateway üstleniyor
app.MapControllers();
app.Run();
