using System;
using System.Threading.Tasks;
using ECHO.Ticket.Business.Interfaces;
using ECHO.Ticket.Core.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECHO.Ticket.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventReviewsController : ControllerBase
{
    private readonly IEventReviewService _reviewService;

    public EventReviewsController(IEventReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    /// <summary>
    /// Bir etkinliğe yeni yorum ve puan ekler. (Giriş yapılması zorunludur)
    /// </summary>
    [HttpPost("add-review")]
    [Authorize] // Token kontrolü zorunlu
    public async Task<IActionResult> AddReview([FromBody] EventReviewCreateDto dto)
    {
        var result = await _reviewService.AddReviewAsync(dto);
        
        if (result.IsSuccess)
            return Ok(result);

        return BadRequest(result);
    }

    /// <summary>
    /// Bir etkinliğe ait tüm yorumları ve puanları listeler. (Herkes görebilir)
    /// </summary>
    [HttpGet("event/{eventId}")]
    public async Task<IActionResult> GetReviewsByEvent(Guid eventId)
    {
        var result = await _reviewService.GetReviewsByEventIdAsync(eventId);
        
        if (result.IsSuccess)
            return Ok(result);

        return BadRequest(result);
    }
    
    [HttpGet("analytics/{eventId:guid}")]
// [Authorize(Roles = "Admin,Organizer")] // Yalnızca admin ve organizatör görsün
    public async Task<IActionResult> GetAnalytics(Guid eventId)
    {
        var result = await _reviewService.GetEventAnalyticsAsync(eventId);
        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }
}