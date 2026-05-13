namespace ECHO.Ticket.Core.DTOs;

public class EventSummaryDto
{
    public Guid EventId { get; set; }
    public string EventName { get; set; } = string.Empty;
    public DateTime EventDate { get; set; }
    
    // İleride etkinlik için toplanan toplam parayı göstermek istersek diye:
    public decimal TotalPledgeAmount { get; set; }
}