// ECHO.Ticket.API/Controllers/DashboardController.cs
using ECHO.Ticket.Business.Interfaces;
using ECHO.Ticket.Core.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECHO.Ticket.API.Controllers;

[Route("api/[controller]")]
[ApiController]
// [Authorize] // İstersen sadece giriş yapmış kullanıcılar görebilsin diye Authorize ekleyebilirsin
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet("summary/{userId:guid}")]
    public async Task<IActionResult> GetSummary(Guid userId)
    {
        var result = await _dashboardService.GetSummaryAsync(userId);

        if (!result.IsSuccess)
        {
            // Result Pattern'inle uyumlu hata dönüşü
            return BadRequest(result);
        }

        // Başarılıysa veriyi dön
        return Ok(result);
    }
}