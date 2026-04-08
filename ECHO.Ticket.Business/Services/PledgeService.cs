using ECHO.Ticket.Business.Interfaces;
using ECHO.Ticket.Core.DTOs;
using ECHO.Ticket.Core.Results;
using ECHO.Ticket.DataAccess.Interfaces;
using FluentValidation;
using Mapster;
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
    private readonly IWorkContext _workContext;

    public PledgeService(
        IRepository<PledgeEntity> pledgeRepository, 
        IRepository<UserEntity> userRepository, 
        IRepository<TicketEntity> ticketRepository,
        IValidator<PledgeEntity> validator, IWorkContext workContext)
    {
        _pledgeRepository = pledgeRepository;
        _userRepository = userRepository;
        _ticketRepository = ticketRepository;
        _validator = validator;
        _workContext = workContext;
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

    public async Task<Result> AddPledgeAsync(PledgeCreateDto pledgeDto)
    {
        var newPledge = pledgeDto.Adapt<PledgeEntity>();
        
        // DTO'dan ne gelirse gelsin önemi yok. Biz Token'daki GÜVENLİ ID'yi kullanıcının ID'si yapıyoruz.
        newPledge.UserId = _workContext.UserId;

        // 1. Veritabanı kontrolleri
        var user = await _userRepository.GetByIdAsync(newPledge.UserId);
        if (user == null)
            return Result.Failure("Hata: Destek yapmak isteyen kullanıcı sistemde bulunamadı!");

        var ticket = await _ticketRepository.GetByIdAsync(newPledge.TicketId);
        if (ticket == null)
            return Result.Failure("Hata: Satın alınmak istenen bilet/paket sistemde bulunamadı!");

        // 2. OTOMATİK FİYAT ATAMASI 
        newPledge.AmountPaid = ticket.Price; // Veritabanındaki biletin kendi fiyatını basıyoruz!
        newPledge.PledgeDate = DateTime.UtcNow;

        // 3. Validasyon ve Kayıt
        var validationResult = await _validator.ValidateAsync(newPledge);
        if (!validationResult.IsValid)
            return Result.Failure(string.Join(" | ", validationResult.Errors.Select(e => e.ErrorMessage)));

        await _pledgeRepository.AddAsync(newPledge);
        await _pledgeRepository.SaveChangesAsync();

        return Result.Success("Satın alma işlemi başarıyla tamamlandı.");
    }
    public async Task<Result> DeletePledgeAsync(Guid id)
    {
        var existingPledge = await _pledgeRepository.GetByIdAsync(id);
        if (existingPledge == null) return Result.Failure("İptal edilecek destek (Pledge) bulunamadı.");

        _pledgeRepository.Remove(existingPledge);
        await _pledgeRepository.SaveChangesAsync();
        return Result.Success("Destek başarıyla iptal edildi.");
    }
}