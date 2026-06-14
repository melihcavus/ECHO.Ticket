namespace ECHO.Ticket.Core.DTOs;

public class SeatSoldEventDto
{
    public Guid EventId { get; set; }
    public string SeatLabel { get; set; } = string.Empty;
}