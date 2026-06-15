using ECHO.Ticket.Core.DTOs;
using ECHO.Ticket.Core.Results;

namespace ECHO.Ticket.Business.Interfaces;

public interface IEventReviewService
{
    Task<Result> AddReviewAsync(EventReviewCreateDto reviewDto);
    Task<Result<IEnumerable<EventReviewDto>>> GetReviewsByEventIdAsync(Guid eventId);
}