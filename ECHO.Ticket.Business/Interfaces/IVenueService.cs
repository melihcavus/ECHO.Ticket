using System.Collections.Generic;
using System.Threading.Tasks;
using ECHO.Ticket.Core.DTOs;
using ECHO.Ticket.Core.Results;

namespace ECHO.Ticket.Business.Interfaces;

public interface IVenueService
{
    Task<Result<IEnumerable<VenueDto>>> GetAllVenuesAsync();
    Task<Result> CreateVenueAsync(CreateVenueDto createVenueDto);
    Task<Result> DeleteVenueAsync(Guid id);
}