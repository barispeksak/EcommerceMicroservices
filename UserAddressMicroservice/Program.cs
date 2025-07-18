// 4. Program.cs (Service Registration)
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using UserAddressMicroservice.Data;
using UserAddressMicroservice.Data.Dtos;
using UserAddressMicroservice.Data.Repositories;
using UserAddressMicroservice.Service.Interfaces;
using UserAddressMicroservice.Service.Services;
using FluentValidation.AspNetCore;
using AutoMapper;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver; // * 
using Serilog; // * 
using UserAddressMicroservice.Middleware; // * 
using UserAddressMicroservice.Http; // * 
using UserAddressMicroservice.Service.Logging; // * 
using Serilog.AspNetCore;
using MassTransit;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddDbContext<UserAddressDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Controllers + Validation
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
    }) // *
    .AddFluentValidation(cfg => cfg.RegisterValidatorsFromAssemblyContaining<Program>());

builder.Services.AddScoped<IUserAddressRepository, UserAddressRepository>();
builder.Services.AddScoped<IUserAddressService, UserAddressService>();

var config   = builder.Configuration;      // ① DI dışı yedek referans

builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((ctx, busCfg) =>
    {
        var rmq = config.GetSection("RabbitMQ");
        string host = rmq["Host"] ?? "rabbitmq"; // Add default value
        string username = rmq["Username"] ?? "guest"; // Add default value
        string password = rmq["Password"] ?? "guest"; // Add default value
        
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

builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly());
builder.Services.AddControllers()
    .AddFluentValidation(fv => fv.RegisterValidatorsFromAssembly(Assembly.GetExecutingAssembly()));

builder.Services.AddHttpClient("UserService", c =>
{
    c.BaseAddress = new Uri("http://usermicroservice:8080/"); // user servis container adı ve port
});

builder.Services.AddHttpClient("AddressService", c =>
{
    c.BaseAddress = new Uri("http://addressservice:8080/"); // address servis container adı ve port
});


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IMongoClient>(sp =>
    new MongoClient("mongodb://mongo:27017")); // veya "localhost" – Docker ortamına göre değişir

builder.Services.AddSingleton<IMongoDatabase>(sp =>
    sp.GetRequiredService<IMongoClient>().GetDatabase("ECommerceLogs"));

builder.Services.AddSingleton<UserAddressActionLogger>();

// Serilog
const string serviceName = "UserAddressTypeMicroservice"; // *
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

//app.UseHttpsRedirection();
app.MapControllers();
app.Run();