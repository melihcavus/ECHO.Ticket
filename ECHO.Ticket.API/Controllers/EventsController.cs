using ECHO.Ticket.Business.Interfaces;
using ECHO.Ticket.Core.Constants;
using ECHO.Ticket.Core.DTOs;
using ECHO.Ticket.Core.Entities;
using Microsoft.AspNetCore.Authorization;
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

    [Authorize(Roles = UserRoles.Organizer + "," + UserRoles.Admin)]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] EventCreateDto eventDto)
    {
        var result = await _eventService.AddEventAsync(eventDto);
        
        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }
    
    [HttpPut]
    public async Task<IActionResult> Update([FromBody] EventUpdateDto eventDto)
    {
        var result = await _eventService.UpdateEventAsync(eventDto);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }
    
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _eventService.DeleteEventAsync(id);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }
    
    [HttpGet("explore")]
    public async Task<IActionResult> GetExploreEvents()
    {
        var result = await _eventService.GetActiveEventsSummaryAsync();
    
        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }
}