// ECHO.Ticket.Core/DTOs/RecentActivityDto.cs
namespace ECHO.Ticket.Core.DTOs;

public class RecentActivityDto
{
    public Guid ActivityId { get; set; }
    public string EventName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public string Type { get; set; } = string.Empty; // Örn: "Bilet Alımı" veya "Proje Desteği"
}