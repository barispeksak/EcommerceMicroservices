// 4. Program.cs (Service Registration)
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using UserAddressMicroservice.Data;
using UserAddressMicroservice.Data.Dtos;
using UserAddressMicroservice.Data.Repositories;
using UserAddressMicroservice.Service.Interfaces;
using UserAddressMicroservice.Service.Services;
using FluentValidation.AspNetCore;
using AutoMapper;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddDbContext<UserAddressDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IUserAddressRepository, UserAddressRepository>();
builder.Services.AddScoped<IUserAddressService, UserAddressService>();

builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly());
builder.Services.AddControllers()
    .AddFluentValidation(fv => fv.RegisterValidatorsFromAssembly(Assembly.GetExecutingAssembly()));

builder.Services.AddHttpClient("UserService", c =>
{
    c.BaseAddress = new Uri("http://usermicroservice:8080/"); // user servis container adı ve port
});

builder.Services.AddHttpClient("AddressService", c =>
{
    c.BaseAddress = new Uri("http://addressservice:8080/"); // address servis container adı ve port
});


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();
app.MapControllers();
app.Run();