using ECHO.Ticket.Core.DTOs;
using ECHO.Ticket.Core.Entities;
using ECHO.Ticket.Core.Results;

namespace ECHO.Ticket.Business.Interfaces;

public interface IUserService
{
    Task<Result<string>> LoginAsync(UserLoginDto loginDto);
    Task<Result<IEnumerable<User>>> GetAllUsersAsync();
    Task<Result<User>> GetUserByIdAsync(Guid id);
    Task<Result> AddUserAsync(UserCreateDto userDto);   
    Task<Result> UpdateUserAsync(UserUpdateDto userDto);
    Task<Result> DeleteUserAsync(Guid id);
    Task<Result> AddBalanceAsync(decimal amount);
    Task<Result> ChangePasswordAsync(ChangePasswordDto request);
    Task<Result> UpdateProfileAsync(UpdateProfileDto request);
}