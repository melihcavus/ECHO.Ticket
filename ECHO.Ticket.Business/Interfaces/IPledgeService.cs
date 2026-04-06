using ECHO.Ticket.Core.DTOs;
using ECHO.Ticket.Core.Results;
using PledgeEntity = ECHO.Ticket.Core.Entities.Pledge;

namespace ECHO.Ticket.Business.Interfaces;

public interface IPledgeService
{
    Task<Result<IEnumerable<PledgeEntity>>> GetAllPledgesAsync();
    Task<Result<PledgeEntity>> GetPledgeByIdAsync(Guid id);
    Task<Result<IEnumerable<PledgeEntity>>> GetPledgesByUserIdAsync(Guid userId); // Bir kullanıcının yaptığı tüm bağışlar
    Task<Result> AddPledgeAsync(PledgeCreateDto pledgeDto);
    Task<Result> DeletePledgeAsync(Guid id);
}