namespace ECHO.Ticket.Core.DTOs;

public class EventReviewCreateDto
{
    public Guid EventId { get; set; }
    public int Rating { get; set; }
    public string Content { get; set; }
}