using ECHO.Ticket.DataAccess.Contexts;
using ECHO.Ticket.DataAccess.Interfaces;
using ECHO.Ticket.DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;
using ECHO.Ticket.Worker;
using Microsoft.AspNetCore.Builder; // WebBuilder için eklendi

// Host.CreateApplicationBuilder YERİNE WebApplication kullanıyoruz ki Render kandırılsın
var builder = WebApplication.CreateBuilder(args);

// Arka plan işçimiz (Worker) eskisi gibi çalışmaya aynen devam edecek
builder.Services.AddHostedService<TicketPurchaseWorker>();

// 1. Veritabanı bağlantısı
builder.Services.AddDbContext<EchoDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Generic Repository
builder.Services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));

var app = builder.Build();

// Render'ın "Bu servis ayakta mı?" kontrolünü geçmesi için uydurma bir ana sayfa
app.MapGet("/", () => "ECHO Worker Service is running and listening to RabbitMQ!");

app.Run();