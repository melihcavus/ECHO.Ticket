namespace ECHO.Ticket.Core.DTOs;

public class EventReviewDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string UserFullName { get; set; } // Mapster ile "FirstName + LastName" eşleyeceğiz
    public int Rating { get; set; }
    public string Content { get; set; }
    public DateTime CreatedAt { get; set; }
}