using ECHO.Ticket.Core.DTOs;
using ECHO.Ticket.Core.Entities;
using ECHO.Ticket.Core.Results;

namespace ECHO.Ticket.Business.Interfaces;

public interface IEventService
{
    Task<Result<IEnumerable<Event>>> GetAllEventsAsync();
    Task<Result<Event>> GetEventByIdAsync(Guid id);
    Task<Result> AddEventAsync(EventCreateDto eventDto); // Sadece başarılı/başarısız döneceği için Data yok (Result)
    Task<Result> UpdateEventAsync(EventUpdateDto eventDto);
    Task<Result> DeleteEventAsync(Guid id);
    Task<Result<IEnumerable<EventSummaryDto>>> GetActiveEventsSummaryAsync();
}