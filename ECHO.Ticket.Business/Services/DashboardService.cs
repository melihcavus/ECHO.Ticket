// ECHO.Ticket.Business/Services/DashboardService.cs
using ECHO.Ticket.Business.Interfaces;
using ECHO.Ticket.Core.DTOs;
using ECHO.Ticket.Core.Entities;
using ECHO.Ticket.Core.Results;
using ECHO.Ticket.DataAccess.Interfaces;

namespace ECHO.Ticket.Business.Services;

public class DashboardService : IDashboardService
{
    private readonly IRepository<Pledge> _pledgeRepository;
    private readonly IRepository<Event> _eventRepository;
    private readonly IRepository<Core.Entities.Ticket> _ticketRepository;

    public DashboardService(
        IRepository<Pledge> pledgeRepository,
        IRepository<Event> eventRepository,
        IRepository<Core.Entities.Ticket> ticketRepository)
    {
        _pledgeRepository = pledgeRepository;
        _eventRepository = eventRepository;
        _ticketRepository = ticketRepository;
    }

    public async Task<Result<DashboardSummaryDto>> GetSummaryAsync(Guid userId)
    {
        try
        {
            var userPledges = await _pledgeRepository.FindAsync(p => p.UserId == userId);
            var userPledgesList = userPledges.ToList(); // Tekrar tekrar sorgulamamak için listeye alıyoruz

            var totalPledgeAmount = userPledgesList.Sum(p => p.AmountPaid); 
            
            var activeProjects = await _eventRepository.FindAsync(e => e.IsActive && e.EventDate > DateTime.UtcNow);
            var activeProjectCount = activeProjects.Count();

            var upcomingEventCount = userPledgesList.Count;

            // --- YENİ EKLENEN KISIM: Son 3 işlemi bul ve listele ---
            var recentPledges = userPledgesList
                .OrderByDescending(p => p.PledgeDate)
                .Take(3)
                .ToList();

            var recentActivities = new List<RecentActivityDto>();

            foreach (var pledge in recentPledges)
            {
                // İlişkili Ticket ve Event bilgilerini senin repository pattern'ine sadık kalarak getiriyoruz
                var ticket = await _ticketRepository.GetByIdAsync(pledge.TicketId);
                var eventEntity = ticket != null ? await _eventRepository.GetByIdAsync(ticket.EventId) : null;

                recentActivities.Add(new RecentActivityDto
                {
                    ActivityId = pledge.Id,
                    EventName = eventEntity?.Title ?? "Bilinmeyen Etkinlik",
                    Amount = pledge.AmountPaid,
                    Date = pledge.PledgeDate,
                    Type = "Bilet / Destek"
                });
            }
            // --------------------------------------------------------

            var summaryDto = new DashboardSummaryDto
            {
                TotalPledgeAmount = totalPledgeAmount,
                ActiveProjectCount = activeProjectCount,
                UpcomingEventCount = upcomingEventCount,
                RecentActivities = recentActivities // Listeyi DTO'ya ekledik
            };

            return Result<DashboardSummaryDto>.Success(summaryDto);
        }
        catch (Exception ex)
        {
            return Result<DashboardSummaryDto>.Failure($"Dashboard verileri alınırken bir hata oluştu: {ex.Message}");
        }
    }
}