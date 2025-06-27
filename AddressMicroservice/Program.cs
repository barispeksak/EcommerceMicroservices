using Microsoft.EntityFrameworkCore;
using AddressMicroservice.Data;
using AddressMicroservice.Data.Repositories;
using AddressMicroservice.Service.Interfaces;
using AddressMicroservice.Service.Services;
using AddressMicroservice.Service.Mapping;
using FluentValidation;
using FluentValidation.AspNetCore;
using AddressMicroservice.Service.Validation;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Add FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssemblyContaining<CreateAddressDtoValidator>();

// Add Entity Framework
builder.Services.AddDbContext<AddressDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

// Add Repository
builder.Services.AddScoped<IAddressRepository, AddressRepository>();

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