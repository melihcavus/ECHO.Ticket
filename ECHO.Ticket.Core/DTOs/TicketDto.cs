// ECHO.Ticket.Core/DTOs/TicketDto.cs
namespace ECHO.Ticket.Core.DTOs;

public class TicketDto
{
    public Guid TicketId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Capacity { get; set; }
    public int RemainingCapacity { get; set; } 
}