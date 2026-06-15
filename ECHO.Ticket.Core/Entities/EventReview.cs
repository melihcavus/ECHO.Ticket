namespace ECHO.Ticket.Core.Entities;

public class EventReview
{
    public Guid Id { get; set; }
    
    // Yorumu yapan kullanıcı
    public Guid UserId { get; set; }
    public User User { get; set; } // Navigation Property
    
    // Yorum yapılan etkinlik
    public Guid EventId { get; set; }
    public Event Event { get; set; } // Navigation Property
    
    // Yıldız Puanı (Örn: 1 ile 5 arası)
    public int Rating { get; set; }
    
    // Kullanıcının yazdığı metin
    public string Content { get; set; }
    
    // Yorumun yapılma tarihi
    public DateTime CreatedAt { get; set; }
}