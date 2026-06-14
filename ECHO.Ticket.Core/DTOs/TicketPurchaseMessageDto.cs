namespace ECHO.Ticket.Core.DTOs;

public class TicketPurchaseMessageDto
{
    public Guid UserId { get; set; }
    public Guid EventId { get; set; }
    public Guid TicketId { get; set; }
    public DateTime RequestDate { get; set; } = DateTime.UtcNow;
    public string? RowLabel { get; set; }
    public int? ColumnNumber { get; set; }
}