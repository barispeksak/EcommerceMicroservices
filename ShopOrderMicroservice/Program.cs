using Microsoft.EntityFrameworkCore;
using ShopOrderMicroservice.Data;
using Npgsql;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Add Entity Framework
builder.Services.AddDbContext<ShopOrderDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add custom services
builder.Services.AddHttpClient("UserService", client =>
{
    client.BaseAddress = new Uri("http://localhost:5056/"); // User mikroservisinin portu
});

builder.Services.AddHttpClient("AddressService", client =>
{
    client.BaseAddress = new Uri("http://localhost:5001/"); // Address mikroservisi
});

builder.Services.AddHttpClient("ProductService", client =>
{
    client.BaseAddress = new Uri("http://localhost:5080/"); // product mikroservisi
});

// Add Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Shop Order Microservice API",
        Version = "v1",
        Description = "A microservice for managing shop orders with full CRUD operations"
    });
});

// Add CORS for microservices communication
builder.Services.AddCors(options =>
{
    options.AddPolicy("MicroservicePolicy",
        policy =>
        {
            policy.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Only use HTTPS redirection in production
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseCors("MicroservicePolicy");
app.UseAuthorization();
app.MapControllers();

app.Run();