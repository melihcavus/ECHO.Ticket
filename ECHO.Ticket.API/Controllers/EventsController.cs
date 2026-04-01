using ECHO.Ticket.Business.Interfaces;
using ECHO.Ticket.Core.Entities;
using Microsoft.AspNetCore.Mvc;

namespace ECHO.Ticket.API.Controllers;

[Route("api/[controller]")]
[ApiController] 
public class EventsController : ControllerBase
{
    private readonly IEventService _eventService;

    public EventsController(IEventService eventService)
    {
        _eventService = eventService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _eventService.GetAllEventsAsync();
        
        if (!result.IsSuccess)
            return BadRequest(result); 

        return Ok(result); 
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _eventService.GetEventByIdAsync(id);
        
        if (!result.IsSuccess)
            return NotFound(result); 

        return Ok(result); 
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Event newEvent)
    {
        var result = await _eventService.AddEventAsync(newEvent);
        
        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }
}