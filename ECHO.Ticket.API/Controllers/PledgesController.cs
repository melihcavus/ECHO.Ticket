using ECHO.Ticket.Business.Interfaces;
using ECHO.Ticket.Core.DTOs;
using Microsoft.AspNetCore.Mvc;
using PledgeEntity = ECHO.Ticket.Core.Entities.Pledge;

namespace ECHO.Ticket.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PledgesController : ControllerBase
{
    private readonly IPledgeService _pledgeService;

    public PledgesController(IPledgeService pledgeService)
    {
        _pledgeService = pledgeService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _pledgeService.GetAllPledgesAsync();
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _pledgeService.GetPledgeByIdAsync(id);
        return result.IsSuccess ? Ok(result) : NotFound(result);
    }

    [HttpGet("user/{userId:guid}")]
    public async Task<IActionResult> GetByUserId(Guid userId)
    {
        var result = await _pledgeService.GetPledgesByUserIdAsync(userId);
        return result.IsSuccess ? Ok(result) : NotFound(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] PledgeCreateDto pledgeDto)
    {
        var result = await _pledgeService.AddPledgeAsync(pledgeDto);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }
    
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _pledgeService.DeletePledgeAsync(id);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }
}