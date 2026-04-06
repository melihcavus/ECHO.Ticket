namespace ECHO.Ticket.Core.DTOs;
public class TicketCreateDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public Guid EventId { get; set; }
    public int Capacity { get; set; }
}