using ECHO.Ticket.Business.Interfaces;
using ECHO.Ticket.Core.Entities;
using ECHO.Ticket.Core.Results;
using ECHO.Ticket.DataAccess.Interfaces;
using FluentValidation;
using TicketEntity = ECHO.Ticket.Core.Entities.Ticket;

namespace ECHO.Ticket.Business.Services;

public class TicketService : ITicketService
{
    private readonly IRepository<TicketEntity> _ticketRepository;
    private readonly IRepository<Event> _eventRepository;
    private readonly IValidator<TicketEntity> _validator; 

    public TicketService(
        IRepository<TicketEntity> ticketRepository, 
        IRepository<Event> eventRepository, 
        IValidator<TicketEntity> validator)
    {
        _ticketRepository = ticketRepository;
        _eventRepository = eventRepository;
        _validator = validator;
    }

    public async Task<Result<IEnumerable<TicketEntity>>> GetAllTicketsAsync()
    {
        var tickets = await _ticketRepository.GetAllAsync();
        return Result<IEnumerable<TicketEntity>>.Success(tickets);
    }

    public async Task<Result<IEnumerable<TicketEntity>>> GetTicketsByEventIdAsync(Guid eventId)
    {
        var allTickets = await _ticketRepository.GetAllAsync();
        var eventTickets = allTickets.Where(t => t.EventId == eventId);

        if (!eventTickets.Any())
        {
            return Result<IEnumerable<TicketEntity>>.Failure("Bu etkinliğe ait henüz bir bilet/paket bulunmuyor.");
        }

        return Result<IEnumerable<TicketEntity>>.Success(eventTickets);
    }

    public async Task<Result<TicketEntity>> GetTicketByIdAsync(Guid id)
    {
        var ticket = await _ticketRepository.GetByIdAsync(id);
        
        if (ticket == null)
            return Result<TicketEntity>.Failure("Belirtilen ID'ye sahip bilet bulunamadı.");
            
        return Result<TicketEntity>.Success(ticket);
    }

    public async Task<Result> AddTicketAsync(TicketEntity newTicket)
    {
        // 1. FluentValidation Kontrolü
        var validationResult = await _validator.ValidateAsync(newTicket);
        if (!validationResult.IsValid)
        {
            var errorMessage = string.Join(" | ", validationResult.Errors.Select(e => e.ErrorMessage));
            return Result.Failure(errorMessage);
        }

        // 2. Veritabanı Kontrolü (Etkinlik var mı?)
        var existingEvent = await _eventRepository.GetByIdAsync(newTicket.EventId);
        if (existingEvent == null)
            return Result.Failure("Hata: Bilet eklemeye çalıştığınız etkinlik veritabanında bulunamadı!");

        await _ticketRepository.AddAsync(newTicket);
        await _ticketRepository.SaveChangesAsync();

        return Result.Success("Bilet/Paket başarıyla oluşturuldu.");
    }
}