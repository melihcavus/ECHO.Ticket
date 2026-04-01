using ECHO.Ticket.Core.Entities;
using ECHO.Ticket.Core.Results;

using TicketEntity = ECHO.Ticket.Core.Entities.Ticket;

namespace ECHO.Ticket.Business.Interfaces;

public interface ITicketService
{
    Task<Result<IEnumerable<TicketEntity>>> GetAllTicketsAsync();
    Task<Result<IEnumerable<TicketEntity>>> GetTicketsByEventIdAsync(Guid eventId); // Sadece bir etkinliğe ait biletleri getirme
    Task<Result<TicketEntity>> GetTicketByIdAsync(Guid id);
    Task<Result> AddTicketAsync(TicketEntity newTicket);
}