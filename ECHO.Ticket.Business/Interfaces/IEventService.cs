using ECHO.Ticket.Core.Entities;
using ECHO.Ticket.Core.Results;

namespace ECHO.Ticket.Business.Interfaces;

public interface IEventService
{
    Task<Result<IEnumerable<Event>>> GetAllEventsAsync();
    Task<Result<Event>> GetEventByIdAsync(Guid id);
    Task<Result> AddEventAsync(Event newEvent); // Sadece başarılı/başarısız döneceği için Data yok (Result)
}