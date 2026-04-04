using ECHO.Ticket.Business.Interfaces;
using ECHO.Ticket.Core.Results;
using ECHO.Ticket.DataAccess.Interfaces;
using PledgeEntity = ECHO.Ticket.Core.Entities.Pledge;
using UserEntity = ECHO.Ticket.Core.Entities.User;
using TicketEntity = ECHO.Ticket.Core.Entities.Ticket;

namespace ECHO.Ticket.Business.Services;

public class PledgeService : IPledgeService
{
    private readonly IRepository<PledgeEntity> _pledgeRepository;
    private readonly IRepository<UserEntity> _userRepository;
    private readonly IRepository<TicketEntity> _ticketRepository;

    public PledgeService(
        IRepository<PledgeEntity> pledgeRepository, 
        IRepository<UserEntity> userRepository, 
        IRepository<TicketEntity> ticketRepository)
    {
        _pledgeRepository = pledgeRepository;
        _userRepository = userRepository;
        _ticketRepository = ticketRepository;
    }

    public async Task<Result<IEnumerable<PledgeEntity>>> GetAllPledgesAsync()
    {
        var pledges = await _pledgeRepository.GetAllAsync();
        return Result<IEnumerable<PledgeEntity>>.Success(pledges);
    }

    public async Task<Result<PledgeEntity>> GetPledgeByIdAsync(Guid id)
    {
        var pledge = await _pledgeRepository.GetByIdAsync(id);
        if (pledge == null)
            return Result<PledgeEntity>.Failure("Belirtilen ID'ye sahip bir destek (Pledge) bulunamadı.");
            
        return Result<PledgeEntity>.Success(pledge);
    }

    public async Task<Result<IEnumerable<PledgeEntity>>> GetPledgesByUserIdAsync(Guid userId)
    {
        var allPledges = await _pledgeRepository.GetAllAsync();
        var userPledges = allPledges.Where(p => p.UserId == userId);

        if (!userPledges.Any())
            return Result<IEnumerable<PledgeEntity>>.Failure("Bu kullanıcının henüz bir desteği bulunmuyor.");

        return Result<IEnumerable<PledgeEntity>>.Success(userPledges);
    }

    public async Task<Result> AddPledgeAsync(PledgeEntity newPledge)
    {
        // İŞ KURALI 1: Kullanıcı gerçekten var mı?
        var user = await _userRepository.GetByIdAsync(newPledge.UserId);
        if (user == null)
            return Result.Failure("Hata: Destek yapmak isteyen kullanıcı sistemde bulunamadı!");

        // İŞ KURALI 2: Bilet (Paket) gerçekten var mı?
        var ticket = await _ticketRepository.GetByIdAsync(newPledge.TicketId);
        if (ticket == null)
            return Result.Failure("Hata: Satın alınmak istenen bilet/paket sistemde bulunamadı!");

        // Her şey yolundaysa bağışı/desteği kaydet
        await _pledgeRepository.AddAsync(newPledge);
        await _pledgeRepository.SaveChangesAsync();

        return Result.Success("Destek işlemi başarıyla tamamlandı. Projeye katkınız için teşekkürler!");
    }
}