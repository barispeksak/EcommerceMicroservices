using Microsoft.EntityFrameworkCore;
using ShopOrderMicroservice.Data;
using ShopOrderMicroservice.Data.Repositories;
using ShopOrderMicroservice.Services.Interfaces;
using ShopOrderMicroservice.Services;
using MongoDB.Driver; // * 
using Serilog; // * 
using ShopOrderMicroservice.Middleware; // * 
using ShopOrderMicroservice.Http; // * 
using ShopOrderMicroservice.Services.Logging; // * 
using Serilog.AspNetCore;
using FluentValidation.AspNetCore;
using MassTransit;
using ShopOrderMicroservice.Consumers;

var builder = WebApplication.CreateBuilder(args);
Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");

// DbContext (PostgreSQL örneği, connection string appsettings.json'dan alınır)
builder.Services.AddDbContext<ShopOrderDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Controllers + Validation
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
    }) // *
    .AddFluentValidation(cfg => cfg.RegisterValidatorsFromAssemblyContaining<Program>());

// MongoDB
builder.Services.AddSingleton<IMongoClient>(sp =>
    new MongoClient("mongodb://mongo:27017")); // * (localhost yazman gerekirse değiştirirsin)
builder.Services.AddSingleton<IMongoDatabase>(sp =>
    sp.GetRequiredService<IMongoClient>().GetDatabase("ECommerceLogs")); // *
builder.Services.AddSingleton<ShopOrderActionLogger>(); // *
var config   = builder.Configuration;      // ① DI dışı yedek referans

builder.Services.AddMassTransit(x =>
{
    // x.AddConsumer<...>();   // varsa consumer kayıtları
    x.AddConsumer<CreateOrderRequestedConsumer>();
    x.AddConsumer<CancelOrderConsumer>(); // Add compensation consumer
    x.UsingRabbitMq((ctx, busCfg) =>
    {
        var rmq = config.GetSection("RabbitMQ");
        busCfg.Host(rmq["Host"] ?? "rabbitmq", "/", h =>
        {
            h.Username(rmq["Username"] ?? "guest");
            h.Password(rmq["Password"] ?? "guest");
        });

        // This will automatically use Common.Contracts.Commands:CreateOrderRequested exchange
        busCfg.ConfigureEndpoints(ctx);
    });
});
// Repository ve Service
builder.Services.AddScoped<IShopOrderRepository, ShopOrderRepository>();
builder.Services.AddScoped<IShopOrderService, ShopOrderService>();

//Inactivate the other microservices.
builder.Services.AddTransient<IHttpClientFactory>(provider => provider.GetRequiredService<IHttpClientFactory>());

// HttpClient (diğer mikroservisler için)
builder.Services.AddHttpClient("UserService", c =>
{
    c.BaseAddress = new Uri("http://usermicroservice:8080/");  // Örnek container adı ve port
});
builder.Services.AddHttpClient("AddressService", c =>
{
    c.BaseAddress = new Uri("http://addressservice:8080/");
});
builder.Services.AddHttpClient("ShippingService", c =>
{
    c.BaseAddress = new Uri("http://shippingtypemicroservice:8080/");
});
builder.Services.AddHttpClient("PaymentService", c =>
{
    c.BaseAddress = new Uri("http://paymenttypemicroservice:8080/");
});
builder.Services.AddHttpClient("ShoppingCartService", c =>
{
    c.BaseAddress = new Uri("http://shoppingcartmicroservice:8080/");
});


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAutoMapper(typeof(Program));

// Serilog
const string serviceName = "ShopOrderMicroservice"; // *
builder.Host.UseSerilog((ctx, svc, cfg) => cfg // *
    .ReadFrom.Configuration(ctx.Configuration) // *
    .Enrich.FromLogContext() // *
    .Enrich.WithProperty("ServiceName", serviceName) // *
    .WriteTo.Console()); // *


var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseRouting(); // *
app.UseSerilogRequestLogging(); // *
app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    if (app.Environment.IsDevelopment() || Environment.GetEnvironmentVariable("APPLY_MIGRATIONS") == "true")
    {
        try
        {
            var shopOrderContext = scope.ServiceProvider.GetRequiredService<ShopOrderDbContext>();
            shopOrderContext.Database.Migrate();
            
            app.Logger.LogInformation("Database migrations applied successfully");
        }
        catch (Exception ex)
        {
            app.Logger.LogError(ex, "An error occurred while applying migrations");
        }
    }
}
app.Run();
