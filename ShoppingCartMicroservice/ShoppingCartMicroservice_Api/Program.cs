using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using ShoppingCartMicroservice_Service.Interfaces;
using ShoppingCartMicroservice_Service.Services;
using FluentValidation.AspNetCore;
using Microsoft.OpenApi.Models;
using FluentValidation;
using ShoppingCartMicroservice_Service.Validation;

var builder = WebApplication.CreateBuilder(args);

// Redis bağlantısı
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var configuration = builder.Configuration.GetConnectionString("Redis");
    if (string.IsNullOrEmpty(configuration))
        throw new InvalidOperationException("Redis connection string missing!");

    var options = ConfigurationOptions.Parse(configuration);
    options.AbortOnConnectFail = false;
    return ConnectionMultiplexer.Connect(options);
});

// Redis distributed cache (opsiyonel)
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
    options.InstanceName = "ecom-cart:";
});

// HttpClient - ProductClient base adres config’den çekiliyor
builder.Services.AddHttpClient<ProductClient>(client =>
{
    var productItemUrl = builder.Configuration["ServiceUrls:ProductItem"];
    client.BaseAddress = new Uri(productItemUrl);
});

// Scoped servis
builder.Services.AddScoped<IShoppingCartService, ShoppingCartService>();

// FluentValidation
builder.Services.AddControllers();
builder.Services.AddFluentValidationAutoValidation()
    .AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssemblyContaining<CreateShoppingCartDtoValidator>();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
