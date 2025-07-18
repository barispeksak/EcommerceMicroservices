using AuthMicroservice.Data;
using AuthMicroservice.Data.Repositories;
using AuthMicroservice.Service.Interfaces;
using AuthMicroservice.Service.Services;
using AuthMicroservice.Service.Validation;
using FluentValidation;
using Microsoft.Extensions.Logging;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using AuthMicroservice.Api;
using AuthMicroservice.Http;
using AuthMicroservice.Middleware;
using MongoDB.Driver;
using Microsoft.Extensions.Configuration;
using MassTransit;

var builder = WebApplication.CreateBuilder(args);


// 5️⃣ JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? builder.Configuration["Jwt__Key"] ?? Environment.GetEnvironmentVariable("Jwt__Key");

if (string.IsNullOrEmpty(jwtKey))
{
    throw new Exception("JWT anahtarı bulunamadı! Ortam değişkenini veya appsettings'i kontrol et.");
}

var keyBytes = Encoding.UTF8.GetBytes(jwtKey);


// 1️⃣ DbContext (PostgreSQL veya SQLite vs. olabilir)
builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2️⃣ Repository ve Service
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();

// 3️⃣ AutoMapper
builder.Services.AddAutoMapper(typeof(Program));

// 4️⃣ FluentValidation
builder.Services.AddControllers();

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssemblyContaining<RegisterDtoValidator>();


builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // Dev ortamı için
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false, // issuer yoksa
        ValidateAudience = false, // audience yoksa
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(keyBytes)
    };
});

var config   = builder.Configuration;      // ① DI dışı yedek referans

builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((ctx, busCfg) =>
    {
        var rmq = builder.Configuration.GetSection("RabbitMQ");
        string host = rmq["Host"] ?? "rabbitmq"; // Default if null
        string username = rmq["Username"] ?? "guest"; // Default if null
        string password = rmq["Password"] ?? "guest"; // Default if null
        
        busCfg.Host(host, "/", h =>
        {
            h.Username(username);
            h.Password(password);
        });

        busCfg.ConfigureEndpoints(ctx);
    });
});

// Add in DI
builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<AuthMicroservice.Http.CorrelationIdDelegatingHandler>();

/* ---------- Serilog ---------- */
builder.Host.UseSerilog((ctx, lc) => lc
    .ReadFrom.Configuration(ctx.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("ServiceName", "AuthMicroservice")
    .WriteTo.Console());

/* ---------- MongoDB DI ---------- */
builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var cfg = sp.GetRequiredService<IConfiguration>();
    var cs  = cfg["MongoDb:ConnectionString"]
              ?? throw new InvalidOperationException("MongoDb:ConnectionString not found!");
    return new MongoClient(cs);
});

builder.Services.AddSingleton<IMongoDatabase>(sp =>
{
    var cfg = sp.GetRequiredService<IConfiguration>();
    var db = cfg["MongoDb:Database"]
              ?? throw new InvalidOperationException("MongoDb:Database not found!");
    return sp.GetRequiredService<IMongoClient>().GetDatabase(db);
});

// Eğer HttpClient kullanıyorsan:
builder.Services.AddHttpClient("SomeService")
    .AddHttpMessageHandler<AuthMicroservice.Http.CorrelationIdDelegatingHandler>();


builder.Services.AddSingleton<AuthLogService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseMiddleware<AuthMicroservice.Middleware.CorrelationIdMiddleware>();

// Apply migrations
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AuthDbContext>();
        context.Database.Migrate();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating the database.");
    }
}


app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthentication(); // JWT önce
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/healthz");
app.Run();