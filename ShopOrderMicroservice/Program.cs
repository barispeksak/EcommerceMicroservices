using Microsoft.EntityFrameworkCore;
using ShopOrderMicroservice.Data;
using ShopOrderMicroservice.Repositories;
using ShopOrderMicroservice.Services.Interfaces;
using ShopOrderMicroservice.Services;

var builder = WebApplication.CreateBuilder(args);

// DbContext (PostgreSQL örneği, connection string appsettings.json'dan alınır)
builder.Services.AddDbContext<ShopOrderDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repository ve Service
builder.Services.AddScoped<IShopOrderRepository, ShopOrderRepository>();
builder.Services.AddScoped<IShopOrderService, ShopOrderService>();

// HttpClient (diğer mikroservisler için)
builder.Services.AddHttpClient("UserService", c =>
{
    c.BaseAddress = new Uri("https://localhost:5001/");
});
builder.Services.AddHttpClient("AddressService", c =>
{
    c.BaseAddress = new Uri("https://localhost:5002/");
});
builder.Services.AddHttpClient("ShippingService", c =>
{
    c.BaseAddress = new Uri("https://localhost:5003/");
});
builder.Services.AddHttpClient("PaymentService", c =>
{
    c.BaseAddress = new Uri("https://localhost:5004/");
});


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAutoMapper(typeof(Program));


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();
app.Run();
