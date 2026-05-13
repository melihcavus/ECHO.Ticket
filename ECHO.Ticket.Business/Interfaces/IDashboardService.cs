using ECHO.Ticket.Core.DTOs;
using ECHO.Ticket.Core.Results;

namespace ECHO.Ticket.Business.Interfaces;

public interface IDashboardService
{
    Task<Result<DashboardSummaryDto>> GetSummaryAsync(Guid userId);
}