using ECHO.Ticket.Business.Interfaces;
using ECHO.Ticket.Core.DTOs;
using ECHO.Ticket.Core.Entities;
using ECHO.Ticket.Core.Results;
using ECHO.Ticket.DataAccess.Interfaces;
using FluentValidation;
using Mapster;

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

    public async Task<Result> AddEventAsync(EventCreateDto eventDto)
    {
        // 1. DTO'yu Entity'e dönüştür
        var newEvent = eventDto.Adapt<Event>();

        // 2. FluentValidation Kontrolü (Artık Entity üzerinden çalışıyor)
        var validationResult = await _validator.ValidateAsync(newEvent);
        if (!validationResult.IsValid)
        {
            var errorMessage = string.Join(" | ", validationResult.Errors.Select(e => e.ErrorMessage));
            return Result.Failure(errorMessage);
        }

        // Her şey yolundaysa kaydet
        await _eventRepository.AddAsync(newEvent);
        await _eventRepository.SaveChangesAsync(); 

        return Result.Success("Etkinlik başarıyla oluşturuldu.");
    }
    public async Task<Result> UpdateEventAsync(EventUpdateDto eventDto)
    {
        var existingEvent = await _eventRepository.GetByIdAsync(eventDto.Id);
        if (existingEvent == null) return Result.Failure("Güncellenecek etkinlik bulunamadı.");

        // Mapster ile DTO'daki verileri mevcut nesnenin üzerine yazıyoruz
        eventDto.Adapt(existingEvent);

        var validationResult = await _validator.ValidateAsync(existingEvent);
        if (!validationResult.IsValid)
        {
            var errorMessage = string.Join(" | ", validationResult.Errors.Select(e => e.ErrorMessage));
            return Result.Failure(errorMessage);
        }

        _eventRepository.Update(existingEvent);
        await _eventRepository.SaveChangesAsync();
        return Result.Success("Etkinlik başarıyla güncellendi.");
    }

    public async Task<Result> DeleteEventAsync(Guid id)
    {
        var existingEvent = await _eventRepository.GetByIdAsync(id);
        if (existingEvent == null) return Result.Failure("Silinecek etkinlik bulunamadı.");

        _eventRepository.Remove(existingEvent);
        await _eventRepository.SaveChangesAsync();
        return Result.Success("Etkinlik başarıyla silindi.");
    }
    public async Task<Result<IEnumerable<EventSummaryDto>>> GetActiveEventsSummaryAsync()
    {
        try
        {
            // Sadece aktif ve tarihi geçmemiş etkinlikleri getir
            var activeEvents = await _eventRepository.FindAsync(e => e.IsActive && e.EventDate > DateTime.UtcNow);
        
            var summaryList = activeEvents.Select(e => new EventSummaryDto
            {
                EventId = e.Id,
                EventName = e.Title, // Entity'deki adın Title olduğunu varsayarak
                EventDate = e.EventDate,
                TotalPledgeAmount = 0 // Şimdilik 0, ileride Pledge tablosuyla birleştirilip hesaplanacak
            }).ToList();

            return Result<IEnumerable<EventSummaryDto>>.Success(summaryList);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<EventSummaryDto>>.Failure($"Etkinlikler getirilirken hata oluştu: {ex.Message}");
        }
    }
}