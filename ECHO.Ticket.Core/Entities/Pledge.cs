namespace ECHO.Ticket.Core.Entities;

public class Pledge
{
    public Guid Id { get; set; }
    public DateTime PledgeDate { get; set; } = DateTime.UtcNow;
    public decimal AmountPaid { get; set; } // Ödenen net tutar

    // İlişkiler (Foreign Key) - İşlemi kim yaptı?
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    // İlişkiler (Foreign Key) - Hangi bileti/paketi satın aldı?
    public Guid TicketId { get; set; }
    public Ticket Ticket { get; set; } = null!;
}