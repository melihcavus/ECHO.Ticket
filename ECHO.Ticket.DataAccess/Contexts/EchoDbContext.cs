using ECHO.Ticket.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace ECHO.Ticket.DataAccess.Contexts;
public class EchoDbContext : DbContext
{
    // API katmanından (Program.cs'den) ayarları alabilmek için gerekli constructor
    public EchoDbContext(DbContextOptions<EchoDbContext> options) : base(options)
    {
    }

    // Veritabanındaki Tablolarımız
    public DbSet<User> Users { get; set; }
    public DbSet<Event> Events { get; set; }
    public DbSet<Core.Entities.Ticket> Tickets { get; set; } 
    //PROJE ADINDA DA TİCKET OLDUĞU İÇİN HANGİSİ OLDUĞUNU ANLAMIYORDU!!
    public DbSet<Pledge> Pledges { get; set; }
}