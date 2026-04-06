namespace ECHO.Ticket.Core.DTOs;

public class TicketUpdateDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Capacity { get; set; }
    // EventId güncellenmez mantığıyla buraya koymuyoruz. Bir bilet başka etkinliğe taşınamaz.
}