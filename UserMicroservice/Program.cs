using Microsoft.EntityFrameworkCore;
using FluentValidation;
using FluentValidation.AspNetCore;
using UserMicroservice.Data;
using UserMicroservice.Dtos;
using UserMicroservice.Service.Mapping;
using UserMicroservice.Service.Validation;
using UserMicroservice.Service.Interfaces;
using UserMicroservice.Service.Services;
using UserMicroservice.Data.Repositories;
using Serilog;
using UserMicroservice.Api;
using UserMicroservice.Http;
using UserMicroservice.Middleware;
using MongoDB.Driver; // <-- Ekle!

var builder = WebApplication.CreateBuilder(args);

// ---------- DB ----------
builder.Services.AddDbContext<UserDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// ---------- Validation ----------
builder.Services.AddControllers();
builder.Services.AddFluentValidationAutoValidation()
                .AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssemblyContaining<CreateUserDtoValidator>();
builder.Services.AddScoped<IValidator<UserDto>, UserDtoValidator>();

// ---------- AutoMapper ----------
builder.Services.AddAutoMapper(typeof(UserProfile));

// ---------- Services & Repos ----------
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

// ---------- MongoDB (UserActionLogger için DI) ----------
builder.Services.AddSingleton<IMongoClient>(sp =>
    new MongoClient("mongodb://mongo:27017")); // <-- Burada docker-compose'da container adı "mongo", localde çalıştırıyorsan "localhost" yapabilirsin

builder.Services.AddSingleton<IMongoDatabase>(sp =>
    sp.GetRequiredService<IMongoClient>().GetDatabase("ECommerceLogs")); // <-- Burada doğru DB adını yaz

builder.Services.AddSingleton<UserActionLogger>(); // <-- Logger servisini ekle

// ---------- HttpClient (UserAddress) ----------
builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<CorrelationIdDelegatingHandler>();
builder.Services.AddHttpClient<IUserAddressApiClient, UserAddressApiClient>(c =>
{
    c.BaseAddress = new Uri("http://useraddressmicroservice");
})
.AddHttpMessageHandler<CorrelationIdDelegatingHandler>();

// ---------- Swagger ----------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ---------- Serilog ----------
const string serviceName = "UserMicroservice";
builder.Host.UseSerilog((ctx, svc, cfg) => cfg
    .ReadFrom.Configuration(ctx.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("ServiceName", serviceName)
    .WriteTo.Console());

// ---------- Build ----------
Console.WriteLine(">>> 1. Program dosyası başladı");
var app = builder.Build();
Console.WriteLine(">>> 2. Host build bitti");

// ---------- Pipeline ----------
app.UseMiddleware<CorrelationIdMiddleware>();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseRouting(); // ekle
app.UseSerilogRequestLogging();
app.UseAuthorization();
app.MapControllers();

app.Run();
