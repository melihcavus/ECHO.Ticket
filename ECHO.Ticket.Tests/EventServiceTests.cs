using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions; // BU SATIR EKLENDİ (Expression kullanımı için şart)
using System.Threading;
using System.Threading.Tasks;
using ECHO.Ticket.Business.Services;
using ECHO.Ticket.Core.DTOs;
using ECHO.Ticket.Core.Entities;
using ECHO.Ticket.Core.Results;
using ECHO.Ticket.DataAccess.Interfaces;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using Xunit;

namespace ECHO.Ticket.Tests;

public class EventServiceTests
{
    private readonly Mock<IRepository<Event>> _eventRepositoryMock;
    private readonly Mock<IValidator<Event>> _validatorMock;
    private readonly Mock<IRepository<Core.Entities.Ticket>> _ticketRepositoryMock;
    private readonly Mock<IRepository<Pledge>> _pledgeRepositoryMock;
    private readonly Mock<IRepository<Venue>> _venueRepositoryMock;
    private readonly EventService _eventService;

    public EventServiceTests()
    {
        _eventRepositoryMock = new Mock<IRepository<Event>>();
        _validatorMock = new Mock<IValidator<Event>>();
        _ticket_repository_initialization();
        _pledge_repository_initialization();
        _venue_repository_initialization();

        _ticketRepositoryMock = new Mock<IRepository<Core.Entities.Ticket>>();
        _pledgeRepositoryMock = new Mock<IRepository<Pledge>>();
        _venueRepositoryMock = new Mock<IRepository<Venue>>();

        _eventService = new EventService(
            _eventRepositoryMock.Object,
            _validatorMock.Object,
            _ticketRepositoryMock.Object,
            _pledgeRepositoryMock.Object,
            _venueRepositoryMock.Object);
    }

    // --- Tests ---

    [Fact]
    public async Task GetAllEventsAsync_ShouldReturnSuccess_WithEvents()
    {
        // Arrange
        var events = new List<Event>
        {
            new Event { Id = Guid.NewGuid(), Title = "E1" },
            new Event { Id = Guid.NewGuid(), Title = "E2" }
        };
        _eventRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(events);

        // Act
        var result = await _eventService.GetAllEventsAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeEquivalentTo(events);
    }

    [Fact]
    public async Task GetEventByIdAsync_ShouldReturnSuccess_WhenEventExists()
    {
        // Arrange
        var id = Guid.NewGuid();
        var ev = new Event { Id = id, Title = "Test" };
        _eventRepositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(ev);

        // Act
        var result = await _event_service_get_by_id_actual(id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().Be(ev);
    }

    [Fact]
    public async Task GetEventByIdAsync_ShouldReturnFailure_WhenEventDoesNotExist()
    {
        // Arrange
        var id = Guid.NewGuid();
        _eventRepositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Event)null);

        // Act
        var result = await _event_service_get_by_id_actual(id);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("Belirtilen ID'ye sahip bir etkinlik bulunamadı.");
    }

    [Fact]
    public async Task AddEventAsync_ShouldReturnSuccess_WhenValidationPasses()
    {
        // Arrange
        var dto = new EventCreateDto { Title = "New", EventDate = DateTime.UtcNow.AddDays(1) };
        _validatorMock.Setup(v => v.ValidateAsync(It.IsAny<Event>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        _eventRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Event>())).Returns(Task.CompletedTask);
        _eventRepositoryMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _eventService.AddEventAsync(dto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Message.Should().Be("Etkinlik başarıyla oluşturuldu.");
        _eventRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Event>()), Times.Once);
        _eventRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task AddEventAsync_ShouldReturnFailure_WhenValidationFails()
    {
        // Arrange
        var dto = new EventCreateDto { Title = "", EventDate = DateTime.UtcNow.AddDays(-1) };
        var failures = new[] { new ValidationFailure("Title", "Başlık gerekli") };
        var validationResult = new ValidationResult(failures);
        _validatorMock.Setup(v => v.ValidateAsync(It.IsAny<Event>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);

        // Act
        var result = await _eventService.AddEventAsync(dto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain("Başlık gerekli");
        _eventRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Event>()), Times.Never);
    }

    [Fact]
    public async Task UpdateEventAsync_ShouldReturnSuccess_WhenEventExistsAndValid()
    {
        // Arrange
        var id = Guid.NewGuid();
        var existing = new Event { Id = id, Title = "Old", EventDate = DateTime.UtcNow.AddDays(1) };
        var dto = new EventUpdateDto { Id = id, Title = "Updated", EventDate = DateTime.UtcNow.AddDays(2) };

        _eventRepositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(existing);
        _validatorMock.Setup(v => v.ValidateAsync(It.IsAny<Event>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        _eventRepositoryMock.Setup(r => r.Update(It.IsAny<Event>()));
        _eventRepositoryMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _eventService.UpdateEventAsync(dto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Message.Should().Be("Etkinlik başarıyla güncellendi.");
        _eventRepositoryMock.Verify(r => r.Update(It.IsAny<Event>()), Times.Once);
        _eventRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateEventAsync_ShouldReturnFailure_WhenEventNotFound()
    {
        // Arrange
        var dto = new EventUpdateDto { Id = Guid.NewGuid(), Title = "X", EventDate = DateTime.UtcNow.AddDays(1) };
        _eventRepositoryMock.Setup(r => r.GetByIdAsync(dto.Id)).ReturnsAsync((Event)null);

        // Act
        var result = await _eventService.UpdateEventAsync(dto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("Güncellenecek etkinlik bulunamadı.");
        _eventRepositoryMock.Verify(r => r.Update(It.IsAny<Event>()), Times.Never);
    }

    [Fact]
    public async Task UpdateEventAsync_ShouldReturnFailure_WhenValidationFails()
    {
        // Arrange
        var id = Guid.NewGuid();
        var existing = new Event { Id = id, Title = "Old", EventDate = DateTime.UtcNow.AddDays(1) };
        var dto = new EventUpdateDto { Id = id, Title = "", EventDate = DateTime.UtcNow.AddDays(-1) };

        var failures = new[] { new ValidationFailure("Title", "Başlık hatası") };
        var validationResult = new ValidationResult(failures);

        _eventRepositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(existing);
        _validatorMock.Setup(v => v.ValidateAsync(It.IsAny<Event>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);

        // Act
        var result = await _eventService.UpdateEventAsync(dto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain("Başlık hatası");
        _eventRepositoryMock.Verify(r => r.Update(It.IsAny<Event>()), Times.Never);
    }

    [Fact]
    public async Task DeleteEventAsync_ShouldReturnSuccess_WhenEventExists()
    {
        // Arrange
        var id = Guid.NewGuid();
        var ev = new Event { Id = id };
        _eventRepositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(ev);
        _eventRepositoryMock.Setup(r => r.Remove(It.IsAny<Event>()));
        _eventRepositoryMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _eventService.DeleteEventAsync(id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Message.Should().Be("Etkinlik başarıyla silindi.");
        _eventRepositoryMock.Verify(r => r.Remove(It.IsAny<Event>()), Times.Once);
        _eventRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteEventAsync_ShouldReturnFailure_WhenEventNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        _eventRepositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Event)null);

        // Act
        var result = await _eventService.DeleteEventAsync(id);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("Silinecek etkinlik bulunamadı.");
        _eventRepositoryMock.Verify(r => r.Remove(It.IsAny<Event>()), Times.Never);
    }

    [Fact]
    public async Task GetActiveEventsSummaryAsync_ShouldReturnSummaries_WhenEventsExist()
    {
        // Arrange
        var future = DateTime.UtcNow.AddDays(5);
        var events = new List<Event>
        {
            new Event { Id = Guid.NewGuid(), Title = "A", EventDate = future, IsActive = true, Category = "K1" },
            new Event { Id = Guid.NewGuid(), Title = "B", EventDate = future.AddDays(1), IsActive = true, Category = "K2" }
        };
        // DÜZELTİLDİ: Func -> Expression<Func>
        _eventRepositoryMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Event, bool>>>())).ReturnsAsync(events);

        // Act
        var result = await _eventService.GetActiveEventsSummaryAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        var list = result.Data!.ToList();
        list.Should().HaveCount(2);
        list[0].EventName.Should().Be("A");
        list[1].Category.Should().Be("K2");
    }

    [Fact]
    public async Task GetActiveEventsSummaryAsync_ShouldReturnFailure_OnException()
    {
        // Arrange
        // DÜZELTİLDİ: Func -> Expression<Func>
        _eventRepositoryMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Event, bool>>>()))
            .ThrowsAsync(new Exception("boom"));

        // Act
        var result = await _eventService.GetActiveEventsSummaryAsync();

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain("Hata: boom");
    }

    [Fact]
    public async Task GetEventDetailAsync_ShouldReturnFailure_WhenEventNullOrInactive()
    {
        // Arrange
        var id = Guid.NewGuid();
        _eventRepositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Event)null);

        // Act
        var resultNull = await _eventService.GetEventDetailAsync(id);

        // Assert
        resultNull.IsSuccess.Should().BeFalse();
        resultNull.Message.Should().Be("Etkinlik bulunamadı veya artık aktif değil.");

        // Arrange inactive
        var inactive = new Event { Id = id, IsActive = false };
        _eventRepositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(inactive);

        // Act
        var resultInactive = await _eventService.GetEventDetailAsync(id);

        // Assert
        resultInactive.IsSuccess.Should().BeFalse();
        resultInactive.Message.Should().Be("Etkinlik bulunamadı veya artık aktif değil.");
    }

    [Fact]
    public async Task GetEventDetailAsync_ShouldReturnDetail_WithVenueAndTickets()
    {
        // Arrange
        var id = Guid.NewGuid();
        var venueId = Guid.NewGuid();
        var ev = new Event
        {
            Id = id,
            Title = "EvTitle",
            Description = "Desc",
            EventDate = DateTime.UtcNow.AddDays(3),
            Location = "Loc",
            Category = "C",
            VenueId = venueId,
            IsActive = true
        };
        var tickets = new List<Core.Entities.Ticket>
        {
            new Core.Entities.Ticket { Id = Guid.NewGuid(), Name = "T1", Description = "d", Price = 10, Capacity = 5, EventId = id, IsActive = true }
        };

        var venue = new Venue { Id = venueId, Name = "V1", Rows = 3, Columns = 3 };

        _eventRepositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(ev);
        // DÜZELTİLDİ: Func -> Expression<Func>
        _ticketRepositoryMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Core.Entities.Ticket, bool>>>())).ReturnsAsync(tickets);
        _venueRepositoryMock.Setup(r => r.GetByIdAsync(venueId)).ReturnsAsync(venue);

        // Act
        var result = await _eventService.GetEventDetailAsync(id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var detail = result.Data!;
        detail.Title.Should().Be("EvTitle");
        detail.VenueName.Should().Be("V1");
        detail.Tickets.Should().HaveCount(1);
        detail.Tickets[0].Name.Should().Be("T1");
        detail.Tickets[0].RemainingCapacity.Should().Be(5);
    }

    [Fact]
    public async Task GetEventDetailAsync_ShouldReturnDetail_WhenVenueIsNull()
    {
        // Arrange
        var id = Guid.NewGuid();
        var ev = new Event
        {
            Id = id,
            Title = "EvTitle",
            Description = "Desc",
            EventDate = DateTime.UtcNow.AddDays(3),
            Location = "Loc",
            Category = "C",
            VenueId = null,
            IsActive = true
        };
        var tickets = new List<Core.Entities.Ticket>();
        _eventRepositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(ev);
        // DÜZELTİLDİ: Func -> Expression<Func>
        _ticketRepositoryMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Core.Entities.Ticket, bool>>>())).ReturnsAsync(tickets);

        // Act
        var result = await _eventService.GetEventDetailAsync(id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var detail = result.Data!;
        detail.VenueId.Should().BeNull();
        detail.Tickets.Should().BeEmpty();
    }

    [Fact]
    public async Task GetEventDetailAsync_ShouldReturnFailure_OnException()
    {
        // Arrange
        var id = Guid.NewGuid();
        _eventRepositoryMock.Setup(r => r.GetByIdAsync(id)).ThrowsAsync(new Exception("boom detail"));

        // Act
        var result = await _eventService.GetEventDetailAsync(id);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain("Hata: boom detail");
    }

    [Fact]
    public async Task GetTakenSeatsAsync_ShouldReturnEmpty_WhenNoTickets()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        // DÜZELTİLDİ: Func -> Expression<Func>
        _ticketRepositoryMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Core.Entities.Ticket, bool>>>()))
            .ReturnsAsync(new List<Core.Entities.Ticket>());

        // Act
        var result = await _eventService.GetTakenSeatsAsync(eventId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeEmpty();
    }

    [Fact]
    public async Task GetTakenSeatsAsync_ShouldReturnTakenSeats_FilteringNulls()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var ticket1 = new Core.Entities.Ticket { Id = Guid.NewGuid(), EventId = eventId };
        var ticket2 = new Core.Entities.Ticket { Id = Guid.NewGuid(), EventId = eventId };
        // DÜZELTİLDİ: Func -> Expression<Func>
        _ticketRepositoryMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Core.Entities.Ticket, bool>>>()))
            .ReturnsAsync(new List<Core.Entities.Ticket> { ticket1, ticket2 });

        var pledges = new List<Pledge>
        {
            new Pledge { Id = Guid.NewGuid(), TicketId = ticket1.Id, RowLabel = "A", ColumnNumber = 1 },
            new Pledge { Id = Guid.NewGuid(), TicketId = ticket1.Id, RowLabel = null, ColumnNumber = 2 }, // filtered out
            new Pledge { Id = Guid.NewGuid(), TicketId = ticket2.Id, RowLabel = "B", ColumnNumber = null } // filtered out
        };
        _pledgeRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(pledges);

        // Act
        var result = await _eventService.GetTakenSeatsAsync(eventId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var seats = result.Data!.ToList();
        seats.Should().HaveCount(1);
        seats[0].Should().Be("A-1");
    }

    [Fact]
    public async Task GetTakenSeatsAsync_ShouldReturnFailure_OnException()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        // DÜZELTİLDİ: Func -> Expression<Func>
        _ticketRepositoryMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Core.Entities.Ticket, bool>>>()))
            .ThrowsAsync(new Exception("tickets boom"));

        // Act
        var result = await _eventService.GetTakenSeatsAsync(eventId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain("Dolu koltuklar alınırken hata oluştu: tickets boom");
    }

    // Private helpers to call service methods directly (keeps test readability)
    private Task<Result<Event>> _event_service_get_by_id_actual(Guid id) => _eventService.GetEventByIdAsync(id);

    // NOTE: The following private initialization methods are only to keep the constructor block compact
    // and do not introduce any artificial helpers for production code. They can be inlined if preferred.
    private void _ticket_repository_initialization() { /* placeholder - no-op */ }
    private void _pledge_repository_initialization() { /* placeholder - no-op */ }
    private void _venue_repository_initialization() { /* placeholder - no-op */ }
}