using ECHO.Ticket.Business.Interfaces;
using ECHO.Ticket.Core.DTOs;
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

    [HttpPost("login")] // İstek adresi: POST /api/Users/login olacak
    public async Task<IActionResult> Login([FromBody] UserLoginDto loginDto)
    {
        // Servisteki LoginAsync metodumuzu çağırıyoruz
        var result = await _userService.LoginAsync(loginDto);

        // Eğer giriş başarısızsa (e-posta yoksa veya şifre yanlışsa) 400 Bad Request dön
        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        // Her şey doğruysa 200 OK ile birlikte üretilen Token'ı dön
        return Ok(result);
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
    public async Task<IActionResult> Create([FromBody] UserCreateDto userDto)
    {
        var result = await _userService.AddUserAsync(userDto);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }
    
    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UserUpdateDto userDto)
    {
        var result = await _userService.UpdateUserAsync(userDto);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _userService.DeleteUserAsync(id);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }
}