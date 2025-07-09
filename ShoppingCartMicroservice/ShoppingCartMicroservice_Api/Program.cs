using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StackExchange.Redis;
using Serilog;
using ShoppingCartMicroservice_Service.Interfaces;
using ShoppingCartMicroservice_Service.Services;
using ShoppingCartMicroservice_Service.Validation;
using FluentValidation;
using FluentValidation.AspNetCore;
using ShoppingCartMicroservice_Api.Middleware;   // CorrelationIdMiddleware
using ShoppingCartMicroservice_Api.Http;         // CorrelationIdDelegatingHandler

var builder = WebApplication.CreateBuilder(args);

/*──────────────────── Serilog (console) ────────────────────*/
const string serviceName = "ShoppingCartMicroservice";
builder.Host.UseSerilog((ctx, lc) => lc
    .ReadFrom.Configuration(ctx.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("ServiceName", serviceName)
    .WriteTo.Console());

/*──────────────────── Redis (cache) ─────────────────────────*/
builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
    ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis")));
builder.Services.AddStackExchangeRedisCache(opt =>
{
    opt.Configuration = builder.Configuration.GetConnectionString("Redis");
    opt.InstanceName = "ecom-cart:";
});

/*──────────────────── Correlation-Id ───────────────────────*/
builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<CorrelationIdDelegatingHandler>();

/*──────────────────── HttpClient → ProductItem ─────────────*/
builder.Services.AddHttpClient<ProductClient>(c =>
{
    c.BaseAddress = new Uri(builder.Configuration["ServiceUrls:ProductItem"]);
}).AddHttpMessageHandler<CorrelationIdDelegatingHandler>();

/*──────────────────── DI + Validation ─────────────────────*/
builder.Services.AddScoped<IShoppingCartService, ShoppingCartService>();

builder.Services.AddControllers();

// FluentValidation ayarı - ayrı ayrı çağrılıyor
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssemblyContaining<CreateShoppingCartDtoValidator>();

/*──────────────────── Swagger ──────────────────────────────*/
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

/*──────────────────── Build & Pipeline ────────────────────*/
var app = builder.Build();

app.UseMiddleware<CorrelationIdMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Burada Authentication ve Authorization yok çünkü gateway hallediyor

app.MapControllers();

app.Run();
