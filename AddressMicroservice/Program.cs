using Microsoft.EntityFrameworkCore;
using AddressMicroservice.Data;
using AddressMicroservice.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Add Entity Framework
builder.Services.AddDbContext<AddressDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add custom services
builder.Services.AddScoped<IAddressService, AddressService>();

// Add Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Address Microservice API",
        Version = "v1",
        Description = "A microservice for managing addresses with full CRUD operations"
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