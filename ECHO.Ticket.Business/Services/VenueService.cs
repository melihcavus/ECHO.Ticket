using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ECHO.Ticket.Business.Interfaces;
using ECHO.Ticket.Core.DTOs;
using ECHO.Ticket.Core.Entities;
using ECHO.Ticket.Core.Results;
using ECHO.Ticket.DataAccess.Contexts;
using Microsoft.EntityFrameworkCore;

namespace ECHO.Ticket.Business.Services;

public class VenueService : IVenueService
{
    private readonly EchoDbContext _context;

    public VenueService(EchoDbContext context)
    {
        _context = context;
    }

    public async Task<Result<IEnumerable<VenueDto>>> GetAllVenuesAsync()
    {
        var venues = await _context.Venues
            .Select(v => new VenueDto
            {
                Id = v.Id,
                Name = v.Name,
                Rows = v.Rows,
                Columns = v.Columns
            }).ToListAsync();

        return Result<IEnumerable<VenueDto>>.Success(venues);
    }

    public async Task<Result> CreateVenueAsync(CreateVenueDto createVenueDto)
    {
        var venue = new Venue
        {
            Name = createVenueDto.Name,
            Rows = createVenueDto.Rows,
            Columns = createVenueDto.Columns
        };

        await _context.Venues.AddAsync(venue);
        await _context.SaveChangesAsync();

        return Result.Success("Sahne başarıyla oluşturuldu.");
    }
}