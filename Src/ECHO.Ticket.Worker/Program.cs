using ECHO.Ticket.DataAccess.Contexts;
using ECHO.Ticket.DataAccess.Interfaces;
using ECHO.Ticket.DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;
using ECHO.Ticket.Worker;

var builder = Host.CreateApplicationBuilder(args);

// Arka plan işçimiz (Worker)
builder.Services.AddHostedService<TicketPurchaseWorker>();

// 1. Veritabanı bağlantısını (DbContext) Render'dan okuyacak şekilde ayarladık
builder.Services.AddDbContext<EchoDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Generic Repository'yi sisteme kaydediyoruz
builder.Services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));

var host = builder.Build();
host.Run();