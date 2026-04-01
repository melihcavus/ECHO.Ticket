using ECHO.Ticket.Business.Interfaces;
using ECHO.Ticket.Core.Entities;
using ECHO.Ticket.Core.Results;
using ECHO.Ticket.DataAccess.Interfaces;
using TicketEntity = ECHO.Ticket.Core.Entities.Ticket;

namespace ECHO.Ticket.Business.Services;

public class TicketService : ITicketService
{
    private readonly IRepository<TicketEntity> _ticketRepository;
    private readonly IRepository<Event> _eventRepository; // Etkinlik var mı diye kontrol etmek için!

    public TicketService(IRepository<TicketEntity> ticketRepository, IRepository<Event> eventRepository)
    {
        _ticketRepository = ticketRepository;
        _eventRepository = eventRepository;
    }

    public async Task<Result<IEnumerable<TicketEntity>>> GetAllTicketsAsync()
    {
        var tickets = await _ticketRepository.GetAllAsync();
        return Result<IEnumerable<TicketEntity>>.Success(tickets);
    }

    public async Task<Result<IEnumerable<TicketEntity>>> GetTicketsByEventIdAsync(Guid eventId)
    {
        // Şimdilik tüm biletleri çekip hafızada filtreliyoruz. 
        // (İleride DataAccess katmanına bir "Where" metodu ekleyip bunu çok daha performanslı yapacağız)
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
        // İŞ KURALI 1: Biletin ekleneceği Etkinlik (Event) gerçekten var mı?
        var existingEvent = await _eventRepository.GetByIdAsync(newTicket.EventId);
        if (existingEvent == null)
        {
            return Result.Failure("Hata: Bilet eklemeye çalıştığınız etkinlik veritabanında bulunamadı!");
        }

        // İŞ KURALI 2: Fiyat ve Kapasite mantıklı mı? (Validasyonların fragmanı)
        if (newTicket.Price < 0) return Result.Failure("Bilet fiyatı 0'dan küçük olamaz.");
        
        // Her şey yolundaysa kaydet
        await _ticketRepository.AddAsync(newTicket);
        await _ticketRepository.SaveChangesAsync();

        return Result.Success("Bilet/Paket başarıyla oluşturuldu.");
    }
}