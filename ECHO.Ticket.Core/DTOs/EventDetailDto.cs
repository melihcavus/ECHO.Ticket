// ECHO.Ticket.Core/DTOs/EventDetailDto.cs
namespace ECHO.Ticket.Core.DTOs;

public class EventDetailDto
{
    public Guid EventId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime EventDate { get; set; }
    public string Location { get; set; } = string.Empty;
    public string OrganizerName { get; set; } = string.Empty; 
    
    // TicketDto'yu ayrı dosyada oluşturduğumuz için burada doğrudan liste olarak çağırabiliyoruz:
    public List<TicketDto> Tickets { get; set; } = new();
}