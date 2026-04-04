using ECHO.Ticket.Business.Interfaces;
using ECHO.Ticket.Core.Entities;
using ECHO.Ticket.Core.Results;
using ECHO.Ticket.DataAccess.Interfaces;
using FluentValidation; 

namespace ECHO.Ticket.Business.Services;

public class EventService : IEventService
{
    private readonly IRepository<Event> _eventRepository;
    private readonly IValidator<Event> _validator; // Validator'ı içeri alıyoruz

    // Constructor'a IValidator'ı ekledik
    public EventService(IRepository<Event> eventRepository, IValidator<Event> validator)
    {
        _eventRepository = eventRepository;
        _validator = validator;
    }

    public async Task<Result<IEnumerable<Event>>> GetAllEventsAsync()
    {
        var events = await _eventRepository.GetAllAsync();
        return Result<IEnumerable<Event>>.Success(events, "Etkinlikler başarıyla listelendi.");
    }

    public async Task<Result<Event>> GetEventByIdAsync(Guid id)
    {
        var eventItem = await _eventRepository.GetByIdAsync(id);
        
        if (eventItem == null)
            return Result<Event>.Failure("Belirtilen ID'ye sahip bir etkinlik bulunamadı.");
        
        return Result<Event>.Success(eventItem);
    }

    public async Task<Result> AddEventAsync(Event newEvent)
    {
        var validationResult = await _validator.ValidateAsync(newEvent);
        
        if (!validationResult.IsValid)
        {
            // Tüm hata mesajlarını virgülle yan yana diziyoruz
            var errorMessage = string.Join(" | ", validationResult.Errors.Select(e => e.ErrorMessage));
            return Result.Failure(errorMessage);
        }
        
        await _eventRepository.AddAsync(newEvent);
        await _eventRepository.SaveChangesAsync(); 

        return Result.Success("Etkinlik başarıyla oluşturuldu.");
    }
}