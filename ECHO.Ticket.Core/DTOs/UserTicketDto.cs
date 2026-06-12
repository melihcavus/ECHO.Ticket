namespace ECHO.Ticket.Core.DTOs;

public class UserTicketDto
{
    public Guid PledgeId { get; set; }
    public string EventTitle { get; set; } = string.Empty;
    public string TicketName { get; set; } = string.Empty;
    public decimal AmountPaid { get; set; }
    public DateTime PledgeDate { get; set; }
}