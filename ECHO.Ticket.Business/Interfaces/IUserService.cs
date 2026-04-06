using ECHO.Ticket.Core.DTOs;
using ECHO.Ticket.Core.Entities;
using ECHO.Ticket.Core.Results;

namespace ECHO.Ticket.Business.Interfaces;

public interface IUserService
{
    Task<Result<IEnumerable<User>>> GetAllUsersAsync();
    Task<Result<User>> GetUserByIdAsync(Guid id);
    Task<Result> AddUserAsync(UserCreateDto userDto);   
    Task<Result> UpdateUserAsync(UserUpdateDto userDto);
    Task<Result> DeleteUserAsync(Guid id);
}