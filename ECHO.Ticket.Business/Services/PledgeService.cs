using ECHO.Ticket.Business.Interfaces;
using ECHO.Ticket.Core.Results;
using ECHO.Ticket.DataAccess.Interfaces;
using FluentValidation;
using PledgeEntity = ECHO.Ticket.Core.Entities.Pledge;
using UserEntity = ECHO.Ticket.Core.Entities.User;
using TicketEntity = ECHO.Ticket.Core.Entities.Ticket;

namespace ECHO.Ticket.Business.Services;

public class PledgeService : IPledgeService
{
    private readonly IRepository<PledgeEntity> _pledgeRepository;
    private readonly IRepository<UserEntity> _userRepository;
    private readonly IRepository<TicketEntity> _ticketRepository;
    private readonly IValidator<PledgeEntity> _validator;

    public PledgeService(
        IRepository<PledgeEntity> pledgeRepository, 
        IRepository<UserEntity> userRepository, 
        IRepository<TicketEntity> ticketRepository,
        IValidator<PledgeEntity> validator)
    {
        _pledgeRepository = pledgeRepository;
        _userRepository = userRepository;
        _ticketRepository = ticketRepository;
        _validator = validator;
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
        // 1. FluentValidation Kontrolü (ID'ler boş mu vb.)
        var validationResult = await _validator.ValidateAsync(newPledge);
        if (!validationResult.IsValid)
        {
            var errorMessage = string.Join(" | ", validationResult.Errors.Select(e => e.ErrorMessage));
            return Result.Failure(errorMessage);
        }

        // 2. Veritabanı Kontrolleri
        var user = await _userRepository.GetByIdAsync(newPledge.UserId);
        if (user == null)
            return Result.Failure("Hata: Destek yapmak isteyen kullanıcı sistemde bulunamadı!");

        var ticket = await _ticketRepository.GetByIdAsync(newPledge.TicketId);
        if (ticket == null)
            return Result.Failure("Hata: Satın alınmak istenen bilet/paket sistemde bulunamadı!");

        await _pledgeRepository.AddAsync(newPledge);
        await _pledgeRepository.SaveChangesAsync();

        return Result.Success("Destek işlemi başarıyla tamamlandı.");
    }
}