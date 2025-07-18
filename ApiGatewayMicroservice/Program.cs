using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Yarp.ReverseProxy;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using ApiGatewayMicroservice.Middleware;
using Serilog;
using ApiGateway.Services;
using MassTransit;

var builder = WebApplication.CreateBuilder(args);

// Add MongoDB log service
builder.Services.AddSingleton<LogService>();

// JWT Ayarı
var jwtKey = builder.Configuration["Jwt:Key"] ?? builder.Configuration["Jwt__Key"] ?? Environment.GetEnvironmentVariable("Jwt__Key");

if (string.IsNullOrEmpty(jwtKey))
{
    throw new Exception("JWT anahtarı bulunamadı! Ortam değişkenini veya appsettings'i kontrol et.");
}

var keyBytes = Encoding.UTF8.GetBytes(jwtKey);


builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
            ValidateLifetime = true
        };
    });

// JWT Policy
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("JwtPolicy", policy =>
    {
        policy.RequireAuthenticatedUser();
    });
});

var config = builder.Configuration;

builder.Services.AddMassTransit(x =>
{
    // x.AddConsumer<...>();

    x.UsingRabbitMq((ctx, busCfg) =>
    {
        var rmq = config.GetSection("RabbitMQ");
        busCfg.Host(rmq["Host"], "/", h =>
        {
            h.Username(rmq["Username"]);
            h.Password(rmq["Password"]);
        });

        busCfg.ConfigureEndpoints(ctx);
    });
});

// YARP yükleniyor
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "API Gateway", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header. Örnek: Bearer {token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    return new MongoClient(config["MongoDb:ConnectionString"]);   // örn: "mongodb://mongo:27017"
});

builder.Services.AddSingleton<IMongoDatabase>(sp =>
{
    var config  = sp.GetRequiredService<IConfiguration>();
    var client  = sp.GetRequiredService<IMongoClient>();
    return client.GetDatabase(config["MongoDb:Database"]);       // örn: "ECommerceLogs"
});

// <–– LogService tüm koleksiyon erişimini kapsülledi ––>
builder.Services.AddSingleton<LogService>();

// ---------------- Serilog ----------------
const string serviceName = "ApiGateway";

builder.Host.UseSerilog((ctx, lc) => lc
    .ReadFrom.Configuration(ctx.Configuration)   // appsettings’teki “Serilog” bölümünü okur
    .Enrich.FromLogContext()
    .Enrich.WithProperty("ServiceName", serviceName)
    // Console sink’i zaten appsettings’te var; burada ekstra eklemek istersen:
    // .WriteTo.Console()
);

var app = builder.Build();

app.UseMiddleware<GatewayLogMiddleware>();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// Swagger UI
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "API Gateway v1");
    c.RoutePrefix = string.Empty;
});

app.MapReverseProxy();
app.Run();

