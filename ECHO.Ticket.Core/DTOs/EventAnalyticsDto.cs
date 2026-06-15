namespace ECHO.Ticket.Core.DTOs;

public class EventAnalyticsDto
{
    public Guid EventId { get; set; }
    public int TotalReviews { get; set; }
    public int PositiveCount { get; set; }
    public int NegativeCount { get; set; }
    public int NeutralCount { get; set; }
    public double SatisfactionScore { get; set; } // % cinsinden memnuniyet skoru
    
    // Kritik müdahale akışı için sadece negatif ve yüksek riskli yorumları tutacağımız liste
    public List<EventReviewDto> CriticalReviews { get; set; } = new();
}