using ECHO.Ticket.Business.Interfaces;
using ECHO.Ticket.Core.Entities;
using Microsoft.AspNetCore.Mvc;

namespace ECHO.Ticket.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _userService.GetAllUsersAsync();
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _userService.GetUserByIdAsync(id);
        return result.IsSuccess ? Ok(result) : NotFound(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] User newUser)
    {
        var result = await _userService.AddUserAsync(newUser);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }
}