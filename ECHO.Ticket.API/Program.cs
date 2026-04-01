using ECHO.Ticket.DataAccess.Contexts;
using ECHO.Ticket.DataAccess.Interfaces;
using ECHO.Ticket.DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Veritabanı bağlantısını sisteme tanıtıyoruz
builder.Services.AddDbContext<EchoDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repository'imizi sisteme tanıtıyoruz
builder.Services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();