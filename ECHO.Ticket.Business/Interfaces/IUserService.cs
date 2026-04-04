using ECHO.Ticket.Core.Entities;
using ECHO.Ticket.Core.Results;

namespace ECHO.Ticket.Business.Interfaces;

public interface IUserService
{
    Task<Result<IEnumerable<User>>> GetAllUsersAsync();
    Task<Result<User>> GetUserByIdAsync(Guid id);
    Task<Result> AddUserAsync(User newUser);
}