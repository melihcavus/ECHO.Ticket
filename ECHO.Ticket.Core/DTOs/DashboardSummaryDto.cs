namespace ECHO.Ticket.Core.DTOs;

public class DashboardSummaryDto
{
    public decimal TotalPledgeAmount { get; set; }
    public int ActiveProjectCount { get; set; }
    public int UpcomingEventCount { get; set; }
    
    public List<RecentActivityDto> RecentActivities { get; set; } = new();
}