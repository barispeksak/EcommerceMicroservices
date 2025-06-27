using trendyolApi.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddDbContext<AppDbContext>(options => // EF Core’un AppDbContext sınıfını servislere tanıtır, böylece veritabanına erişim sağlar.
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));  // postgreSQL ***

builder.Services.AddControllers(); // RESTful API'de klasik controller yapısını kullanabilmek için gerekli altyapıyı sağlar.
builder.Services.AddEndpointsApiExplorer(); // Swagger dokümantasyonu için gerekli servisleri etkinleştirir.
builder.Services.AddSwaggerGen(); // Swagger arayüzü (UI) için gerekli yapılandırmayı ekler.

builder.Services.AddHttpClient("AddressService", client =>
{
    client.BaseAddress = new Uri("http://localhost:5001/"); // adres mikroservisinin base URL'si
});

var app = builder.Build(); // Uygulamayı başlatmak için builder nesnesinden bir app nesnesi oluşturur.

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger(); // klasik Swagger yapısı
    app.UseSwaggerUI();
}

app.UseHttpsRedirection(); // HTTP isteklerini otomatik olarak HTTPS’e yönlendirir.
app.MapControllers();

app.Run();
