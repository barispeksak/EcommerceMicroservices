using Microsoft.EntityFrameworkCore;
using FluentValidation;
using FluentValidation.AspNetCore;
using UserMicroservice.Data;
using UserMicroservice.Service;
using UserMicroservice.Dtos;
using UserMicroservice.Service.Mapping;
using UserMicroservice.Service.Validation;
using UserMicroservice.Service.Interfaces;
using UserMicroservice.Service.Services;
using UserMicroservice.Data.Repositories;

var builder = WebApplication.CreateBuilder(args);

// --- EF Core ---
builder.Services.AddDbContext<UserDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// --- FluentValidation ---
builder.Services.AddControllers()
    .AddFluentValidation(fv =>
    {
        fv.RegisterValidatorsFromAssemblyContaining<UserDtoValidator>();
        fv.RegisterValidatorsFromAssemblyContaining<CreateUserDtoValidator>();
        fv.AutomaticValidationEnabled = true; // 🔍 Bu şart!
    });

builder.Services.AddScoped<IValidator<UserDto>, UserDtoValidator>(); // Validator bağlanır

// --- AutoMapper ---
builder.Services.AddAutoMapper(typeof(UserProfile)); // AutoMapper yapılandırması (Profile sınıfı)

// --- Service katmanı ---
builder.Services.AddScoped<IUserService, UserService>(); // Service katmanı bağlanır
builder.Services.AddScoped<IUserRepository, UserRepository>();


// --- Swagger ---
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// --- HTTP Pipeline ---
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();
app.Run();
