using System.Threading.Tasks;
using ECHO.Ticket.Business.Interfaces;
using ECHO.Ticket.Core.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECHO.Ticket.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin,Organizer")]
public class VenuesController : ControllerBase
{
    private readonly IVenueService _venueService;

    public VenuesController(IVenueService venueService)
    {
        _venueService = venueService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll()
    {
        var result = await _venueService.GetAllVenuesAsync();
        if (result.IsSuccess)
            return Ok(result);
            
        return BadRequest(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateVenueDto createVenueDto)
    {
        var result = await _venueService.CreateVenueAsync(createVenueDto);
        if (result.IsSuccess)
            return Ok(result);
            
        return BadRequest(result);
    }
}