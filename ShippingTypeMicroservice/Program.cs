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
using MongoDB.Driver; // * 
using Serilog; // * 
using ShippingTypeMicroservice.Middleware; // * 
using ShippingTypeMicroservice.Http; // * 
using ShippingTypeMicroservice.Service.Logging; // * 
using Serilog.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// DbContext
builder.Services.AddDbContext<ShippingDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Controllers + Validation
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
    }) // *
    .AddFluentValidation(cfg => cfg.RegisterValidatorsFromAssemblyContaining<Program>());

// AutoMapper
builder.Services.AddAutoMapper(typeof(ShippingTypeProfile));

// Services
builder.Services.AddScoped<IShippingTypeService, ShippingTypeService>();
builder.Services.AddScoped<IShippingTypeRepository, ShippingTypeRepository>();

// MongoDB
builder.Services.AddSingleton<IMongoClient>(sp =>
    new MongoClient("mongodb://mongo:27017")); // * (localhost yazman gerekirse değiştirirsin)
builder.Services.AddSingleton<IMongoDatabase>(sp =>
    sp.GetRequiredService<IMongoClient>().GetDatabase("ECommerceLogs")); // *
builder.Services.AddSingleton<ShippingActionLogger>(); // *

// // HttpClient
// builder.Services.AddHttpContextAccessor(); // *
// builder.Services.AddTransient<CorrelationIdDelegatingHandler>(); // *
// builder.Services.AddHttpClient<IShippingTrackingApiClient, ShippingTrackingApiClient>(c => // örnek api client
// {
//     c.BaseAddress = new Uri("http://shippingtrackingmicroservice");
// }) // *
// .AddHttpMessageHandler<CorrelationIdDelegatingHandler>(); // *

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Serilog
const string serviceName = "ShippingTypeMicroservice"; // *
builder.Host.UseSerilog((ctx, svc, cfg) => cfg // *
    .ReadFrom.Configuration(ctx.Configuration) // *
    .Enrich.FromLogContext() // *
    .Enrich.WithProperty("ServiceName", serviceName) // *
    .WriteTo.Console()); // *

var app = builder.Build();


app.UseSwagger();
app.UseSwaggerUI();


app.UseMiddleware<CorrelationIdMiddleware>(); // *

app.UseHttpsRedirection();
app.UseRouting(); // *
app.UseSerilogRequestLogging(); // *
app.UseAuthorization();
app.MapControllers();

app.Run();
