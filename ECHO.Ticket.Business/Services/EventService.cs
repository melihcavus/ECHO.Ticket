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
    private readonly IValidator<Event> _validator;
    private readonly IRepository<Core.Entities.Ticket> _ticketRepository;
    private readonly IRepository<Pledge> _pledgeRepository;
    private readonly IRepository<Venue> _venueRepository;

    // Somut sınıflara (concrete) değil, soyutlamalara (interface) bağımlılık (DIP)
    public EventService(IRepository<Event> eventRepository, IValidator<Event> validator,
        IRepository<Core.Entities.Ticket> ticketRepository, IRepository<Pledge> pledgeRepository,
        IRepository<Venue> venueRepository)
    {
        _eventRepository = eventRepository;
        _validator = validator;
        _ticketRepository = ticketRepository;
        _pledgeRepository = pledgeRepository;
        _venueRepository = venueRepository;
    }

    public async Task<Result<IEnumerable<Event>>> GetAllEventsAsync()
    {
        var events = await _eventRepository.GetAllAsync();
        return Result<IEnumerable<Event>>.Success(events);
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
        var newEvent = eventDto.Adapt<Event>();
        newEvent.VenueId = eventDto.VenueId;
        
        // IValidator kontratına uyan herhangi bir sınıf burada sistemi bozmadan çalışabilir (LSP)
        var validationResult = await _validator.ValidateAsync(newEvent);
        
        if (!validationResult.IsValid)
        {
            var errorMessage = string.Join(" | ", validationResult.Errors.Select(e => e.ErrorMessage));
            return Result.Failure(errorMessage);
        }

        await _eventRepository.AddAsync(newEvent);
        await _eventRepository.SaveChangesAsync(); 

        return Result.Success("Etkinlik başarıyla oluşturuldu.");
    }

    public async Task<Result> UpdateEventAsync(EventUpdateDto eventDto)
    {
        var existingEvent = await _eventRepository.GetByIdAsync(eventDto.Id);
        if (existingEvent == null) return Result.Failure("Güncellenecek etkinlik bulunamadı.");

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
            var activeEvents = await _eventRepository.FindAsync(e => e.IsActive && e.EventDate > DateTime.UtcNow);
        
            var summaryList = activeEvents.Select(e => new EventSummaryDto
            {
                EventId = e.Id,
                EventName = e.Title,
                EventDate = e.EventDate,
                Category = e.Category,
                TotalPledgeAmount = 0
            }).ToList();

            return Result<IEnumerable<EventSummaryDto>>.Success(summaryList);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<EventSummaryDto>>.Failure($"Hata: {ex.Message}");
        }
    }

    public async Task<Result<EventDetailDto>> GetEventDetailAsync(Guid id)
    {
        try
        {
            var eventEntity = await _eventRepository.GetByIdAsync(id);
            if (eventEntity == null || !eventEntity.IsActive)
                return Result<EventDetailDto>.Failure("Etkinlik bulunamadı veya artık aktif değil.");

            var tickets = await _ticketRepository.FindAsync(t => t.EventId == id && t.IsActive);
            
            Venue? venue = null;
            if (eventEntity.VenueId.HasValue)
            {
                venue = await _venueRepository.GetByIdAsync(eventEntity.VenueId.Value);
            }

            var eventDetail = new EventDetailDto
            {
                EventId = eventEntity.Id,
                Title = eventEntity.Title,
                Description = eventEntity.Description,
                EventDate = eventEntity.EventDate,
                Location = eventEntity.Location,
                OrganizerName = "Organizatör",
                Category = eventEntity.Category,
                VenueId = venue?.Id,
                VenueName = venue?.Name,
                VenueRows = venue?.Rows,
                VenueColumns = venue?.Columns,
                Tickets = tickets.Select(t => new TicketDto
                {
                    TicketId = t.Id,
                    Name = t.Name,
                    Description = t.Description,
                    Price = t.Price,
                    Capacity = t.Capacity,
                    RemainingCapacity = t.Capacity 
                }).ToList()
            };

            return Result<EventDetailDto>.Success(eventDetail);
        }
        catch (Exception ex)
        {
            return Result<EventDetailDto>.Failure($"Hata: {ex.Message}");
        }
    }
    public async Task<Result<IEnumerable<string>>> GetTakenSeatsAsync(Guid eventId)
    {
        try
        {
            // Önce bu etkinliğe ait tüm biletleri bul
            var tickets = await _ticketRepository.FindAsync(t => t.EventId == eventId);
            var ticketIds = tickets.Select(t => t.Id).ToList();

            if (!ticketIds.Any())
                return Result<IEnumerable<string>>.Success(new List<string>());

            // Sonra bu biletlerle yapılmış olan satışları (Pledges) bul
            var allPledges = await _pledgeRepository.GetAllAsync();
            var takenSeats = allPledges
                .Where(p => ticketIds.Contains(p.TicketId) && !string.IsNullOrEmpty(p.RowLabel) && p.ColumnNumber.HasValue)
                .Select(p => $"{p.RowLabel}-{p.ColumnNumber}") // Örn: "A-5"
                .ToList();

            return Result<IEnumerable<string>>.Success(takenSeats);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<string>>.Failure($"Dolu koltuklar alınırken hata oluştu: {ex.Message}");
        }
    }
}