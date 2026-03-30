namespace ECHO.Ticket.Core.Entities;

public class Ticket
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty; // Örn: Standart Bilet, VIP, Kurucu Destekçisi
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Capacity { get; set; } // Bu spesifik bilet/paket türünden kaç adet satılabilir?
    public bool IsActive { get; set; } = true;

    // İlişkiler (Foreign Key) - Bu bilet hangi etkinliğe ait?
    public Guid EventId { get; set; }
    public Event Event { get; set; } = null!;
}
