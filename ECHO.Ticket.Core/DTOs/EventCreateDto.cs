namespace ECHO.Ticket.Core.DTOs;
public class EventCreateDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime EventDate { get; set; }
    public Guid OrganizerId { get; set; }
    public string Location { get; set; } = string.Empty;
}