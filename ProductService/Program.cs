using ProductService.Data;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;


var builder = WebApplication.CreateBuilder(args);

// 🔁 PostgreSQL bağlantı
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddSwaggerGen(c =>
{
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.Configure<ApiBehaviorOptions>(opt =>
{
    opt.InvalidModelStateResponseFactory = ctx =>
    {
        var errors = ctx.ModelState
                        .Where(e => e.Value!.Errors.Any())
                        .Select(e => new {
                            Field = e.Key,
                            Messages = e.Value!.Errors
                                              .Select(x => x.ErrorMessage)
                        });

        return new BadRequestObjectResult(new {
            message = "Model doğrulama hatası",
            errors
        });
    };
});


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();
app.Run();



