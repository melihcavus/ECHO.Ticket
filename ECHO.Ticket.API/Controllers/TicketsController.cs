using ECHO.Ticket.Business.Interfaces;
using ECHO.Ticket.Business.RabbitMQ;
using ECHO.Ticket.Core.DTOs;
using ECHO.Ticket.API.Hubs;
using Microsoft.AspNetCore.SignalR;
using ECHO.Ticket.Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TicketEntity = ECHO.Ticket.Core.Entities.Ticket;
namespace ECHO.Ticket.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TicketsController : ControllerBase
{
    private readonly ITicketService _ticketService;
    private readonly IMessageProducer _messageProducer;

    public TicketsController(ITicketService ticketService, IMessageProducer messageProducer)
    {
        _ticketService = ticketService;
        _messageProducer = messageProducer;
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
    
    // Bilet Satın Alma Endpoint'i
    [HttpPost("purchase")]
    public async Task<IActionResult> PurchaseTicket([FromBody] TicketPurchaseMessageDto request)
    {
        // 1. Gerçek senaryoda UserId'yi Token'dan (User.Claims) alırız ama şimdilik DTO'dan okuyoruz.
    
        // 2. İşlemi RabbitMQ kuyruğuna fırlatıyoruz!
        string queueName = "ticket_purchase_queue";
        await _messageProducer.SendMessageAsync(request, queueName);

        // 3. Kullanıcıya "İşleminiz sıraya alındı" mesajı dönüyoruz (Bekletmiyoruz!)
        return Accepted(new { isSuccess = true, message = "Bilet alma talebiniz sıraya alındı. İşleminiz arka planda tamamlanacak." });
    }
    [HttpPost("broadcast-seat")]
    [AllowAnonymous]
    public async Task<IActionResult> BroadcastSeat([FromBody] SeatSoldEventDto dto, [FromServices] IHubContext<TicketHub> hubContext)
    {
        await hubContext.Clients.All.SendAsync("SeatSold", dto.EventId, dto.SeatLabel);
        return Ok();
    }
}