using ECHO.Ticket.Business.Interfaces;
using ECHO.Ticket.Core.Entities;
using ECHO.Ticket.Core.Results;
using ECHO.Ticket.DataAccess.Interfaces;

namespace ECHO.Ticket.Business.Services;

public class EventService : IEventService
{
    private readonly IRepository<Event> _eventRepository;

    public EventService(IRepository<Event> eventRepository)
    {
        _eventRepository = eventRepository;
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
        {
            // Veritabanında yoksa null dönmek yerine zarfın içine hata mesajı koyuyoruz
            return Result<Event>.Failure("Belirtilen ID'ye sahip bir etkinlik bulunamadı.");
        }
        
        return Result<Event>.Success(eventItem);
    }

    public async Task<Result> AddEventAsync(Event newEvent)
    {
        // İŞ KURALI (Business Rule) 1: Tarih geçmişte olamaz
        if (newEvent.EventDate < DateTime.UtcNow) 
        {
            return Result.Failure("Etkinlik tarihi geçmiş bir tarih olamaz.");
        }

        await _eventRepository.AddAsync(newEvent);
        await _eventRepository.SaveChangesAsync(); 

        return Result.Success("Etkinlik başarıyla oluşturuldu.");
    }
}