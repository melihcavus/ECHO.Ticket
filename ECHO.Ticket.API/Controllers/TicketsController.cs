using ECHO.Ticket.Business.Interfaces;
using ECHO.Ticket.Core.DTOs;
using ECHO.Ticket.Core.Entities;
using Microsoft.AspNetCore.Mvc;
using TicketEntity = ECHO.Ticket.Core.Entities.Ticket;
namespace ECHO.Ticket.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TicketsController : ControllerBase
{
    private readonly ITicketService _ticketService;

    public TicketsController(ITicketService ticketService)
    {
        _ticketService = ticketService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _ticketService.GetAllTicketsAsync();
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpGet("event/{eventId:guid}")]
    public async Task<IActionResult> GetByEventId(Guid eventId)
    {
        var result = await _ticketService.GetTicketsByEventIdAsync(eventId);
        return result.IsSuccess ? Ok(result) : NotFound(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _ticketService.GetTicketByIdAsync(id);
        return result.IsSuccess ? Ok(result) : NotFound(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] TicketCreateDto ticketDto)
    {
        var result = await _ticketService.AddTicketAsync(ticketDto);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }
    
    [HttpPut]
    public async Task<IActionResult> Update([FromBody] TicketUpdateDto ticketDto)
    {
        var result = await _ticketService.UpdateTicketAsync(ticketDto);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _ticketService.DeleteTicketAsync(id);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }
}