namespace ECHO.Ticket.Core.Entities;

public class Event
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime EventDate { get; set; } 
    public string Location { get; set; } = string.Empty;

    //Fonlama/Biletleme Hedefleri
    public int TotalTicketsCapacity { get; set; }
    public decimal TicketPrice { get; set; }
    
    //Durum Kontrolü
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    //İlişkiler (Foregin Key) - Bu etkinliği kim oluşturdu?
    public Guid OrganizerId { get; set; }
    public User Organizer { get; set; } = null!;
}