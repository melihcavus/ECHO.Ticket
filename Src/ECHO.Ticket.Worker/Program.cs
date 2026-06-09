using ECHO.Ticket.DataAccess.Contexts;
using ECHO.Ticket.DataAccess.Interfaces;
using ECHO.Ticket.DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;
using ECHO.Ticket.Worker;

var builder = Host.CreateApplicationBuilder(args);

// Arka plan işçimiz (Worker)
builder.Services.AddHostedService<TicketPurchaseWorker>();

// 1. Veritabanı bağlantısını (DbContext) Worker'a da tanıtıyoruz
// Docker'daki PostgreSQL bilgilerini giriyoruz
builder.Services.AddDbContext<EchoDbContext>(options =>
    options.UseNpgsql("Host=localhost;Port=5432;Database=EchoTicketDb;Username=postgres;Password=123456"));

// 2. Generic Repository'yi sisteme kaydediyoruz
builder.Services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));

var host = builder.Build();
host.Run();