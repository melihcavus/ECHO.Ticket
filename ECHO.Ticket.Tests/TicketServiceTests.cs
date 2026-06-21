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
using TicketEntity = ECHO.Ticket.Core.Entities.Ticket;

namespace ECHO.Ticket.Tests;

public class TicketServiceTests
{
    private readonly Mock<IRepository<TicketEntity>> _ticketRepositoryMock;
    private readonly Mock<IRepository<Event>> _eventRepositoryMock;
    private readonly Mock<IRepository<Venue>> _venueRepositoryMock;
    private readonly Mock<IValidator<TicketEntity>> _validatorMock;
    private readonly TicketService _ticketService;

    public TicketServiceTests()
    {
        _ticketRepositoryMock = new Mock<IRepository<TicketEntity>>();
        _eventRepositoryMock = new Mock<IRepository<Event>>();
        _venueRepositoryMock = new Mock<IRepository<Venue>>();
        _validatorMock = new Mock<IValidator<TicketEntity>>();

        _ticketService = new TicketService(
            _ticketRepositoryMock.Object,
            _eventRepositoryMock.Object,
            _venueRepositoryMock.Object,
            _validatorMock.Object);
    }

    #region GetAllTicketsAsync Tests

    [Fact]
    public async Task GetAllTicketsAsync_ShouldReturnSuccess_WhenTicketsExist()
    {
        // Arrange
        var tickets = new List<TicketEntity>
        {
            new TicketEntity { Id = Guid.NewGuid(), Name = "Standard", Price = 100, Capacity = 50, EventId = Guid.NewGuid() },
            new TicketEntity { Id = Guid.NewGuid(), Name = "VIP", Price = 200, Capacity = 20, EventId = Guid.NewGuid() }
        };
        _ticketRepositoryMock.Setup(repo => repo.GetAllAsync()).ReturnsAsync(tickets);

        // Act
        var result = await _ticketService.GetAllTicketsAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Should().HaveCount(2);
        result.Data.Should().BeEquivalentTo(tickets);
        _ticketRepositoryMock.Verify(repo => repo.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetAllTicketsAsync_ShouldReturnSuccess_WhenNoTicketsExist()
    {
        // Arrange
        var emptyTickets = new List<TicketEntity>();
        _ticketRepositoryMock.Setup(repo => repo.GetAllAsync()).ReturnsAsync(emptyTickets);

        // Act
        var result = await _ticketService.GetAllTicketsAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Should().BeEmpty();
    }

    #endregion

    #region GetTicketByIdAsync Tests

    [Fact]
    public async Task GetTicketByIdAsync_ShouldReturnSuccess_WhenTicketExists()
    {
        // Arrange
        var ticketId = Guid.NewGuid();
        var expectedTicket = new TicketEntity
        {
            Id = ticketId,
            Name = "Standard",
            Price = 100,
            Capacity = 50,
            EventId = Guid.NewGuid()
        };
        _ticketRepositoryMock.Setup(repo => repo.GetByIdAsync(ticketId)).ReturnsAsync(expectedTicket);

        // Act
        var result = await _ticketService.GetTicketByIdAsync(ticketId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Should().Be(expectedTicket);
        result.Message.Should().BeEmpty();
    }

    [Fact]
    public async Task GetTicketByIdAsync_ShouldReturnFailure_WhenTicketDoesNotExist()
    {
        // Arrange
        var ticketId = Guid.NewGuid();
        _ticketRepositoryMock.Setup(repo => repo.GetByIdAsync(ticketId)).ReturnsAsync((TicketEntity)null);

        // Act
        var result = await _ticketService.GetTicketByIdAsync(ticketId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Data.Should().BeNull();
        result.Message.Should().Be("Belirtilen ID'ye sahip bilet bulunamadı.");
    }

    #endregion

    #region GetTicketsByEventIdAsync Tests

    [Fact]
    public async Task GetTicketsByEventIdAsync_ShouldReturnSuccess_WhenTicketsExistForEvent()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var tickets = new List<TicketEntity>
        {
            new TicketEntity { Id = Guid.NewGuid(), Name = "Standard", Price = 100, Capacity = 50, EventId = eventId },
            new TicketEntity { Id = Guid.NewGuid(), Name = "VIP", Price = 200, Capacity = 20, EventId = eventId }
        };
        _ticketRepositoryMock.Setup(repo => repo.GetAllAsync()).ReturnsAsync(tickets);

        // Act
        var result = await _ticketService.GetTicketsByEventIdAsync(eventId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Should().HaveCount(2);
        result.Data.Should().AllSatisfy(t => t.EventId.Should().Be(eventId));
    }

    [Fact]
    public async Task GetTicketsByEventIdAsync_ShouldReturnFailure_WhenNoTicketsExistForEvent()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var otherEventId = Guid.NewGuid();
        var tickets = new List<TicketEntity>
        {
            new TicketEntity { Id = Guid.NewGuid(), Name = "Standard", Price = 100, Capacity = 50, EventId = otherEventId }
        };
        _ticketRepositoryMock.Setup(repo => repo.GetAllAsync()).ReturnsAsync(tickets);

        // Act
        var result = await _ticketService.GetTicketsByEventIdAsync(eventId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Data.Should().BeNull();
        result.Message.Should().Be("Bu etkinliğe ait henüz bir bilet/paket bulunmuyor.");
    }

    [Fact]
    public async Task GetTicketsByEventIdAsync_ShouldReturnFailure_WhenNoTicketsExistAtAll()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var emptyTickets = new List<TicketEntity>();
        _ticketRepositoryMock.Setup(repo => repo.GetAllAsync()).ReturnsAsync(emptyTickets);

        // Act
        var result = await _ticketService.GetTicketsByEventIdAsync(eventId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("Bu etkinliğe ait henüz bir bilet/paket bulunmuyor.");
    }

    #endregion

    #region AddTicketAsync Tests

    [Fact]
    public async Task AddTicketAsync_ShouldReturnSuccess_WhenTicketIsValidAndEventExists()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var ticketDto = new TicketCreateDto
        {
            Name = "Standard",
            Description = "Standard Ticket",
            Price = 100,
            Capacity = 50,
            EventId = eventId
        };

        var ticketEntity = new TicketEntity
        {
            Name = ticketDto.Name,
            Description = ticketDto.Description,
            Price = ticketDto.Price,
            Capacity = ticketDto.Capacity,
            EventId = ticketDto.EventId
        };

        var existingEvent = new Event { Id = eventId, Title = "Test Event", VenueId = null };
        var validationResult = new ValidationResult();

        _validatorMock.Setup(v => v.ValidateAsync(It.IsAny<TicketEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);
        _eventRepositoryMock.Setup(repo => repo.GetByIdAsync(eventId)).ReturnsAsync(existingEvent);
        _ticketRepositoryMock.Setup(repo => repo.AddAsync(It.IsAny<TicketEntity>())).Returns(Task.CompletedTask);
        _ticketRepositoryMock.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _ticketService.AddTicketAsync(ticketDto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Message.Should().Be("Bilet/Paket başarıyla oluşturuldu.");
        _ticketRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<TicketEntity>()), Times.Once);
        _ticketRepositoryMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task AddTicketAsync_ShouldReturnFailure_WhenValidationFails()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var ticketDto = new TicketCreateDto
        {
            Name = "Standard",
            Description = "Standard Ticket",
            Price = -50, // Invalid price
            Capacity = 50,
            EventId = eventId
        };

        var validationFailure = new ValidationFailure("Price", "Bilet/Paket fiyatı 0'dan küçük olamaz.");
        var validationResult = new ValidationResult(new[] { validationFailure });

        _validatorMock.Setup(v => v.ValidateAsync(It.IsAny<TicketEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);

        // Act
        var result = await _ticketService.AddTicketAsync(ticketDto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain("Bilet/Paket fiyatı 0'dan küçük olamaz.");
        _ticketRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<TicketEntity>()), Times.Never);
    }

    [Fact]
    public async Task AddTicketAsync_ShouldReturnFailure_WhenEventDoesNotExist()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var ticketDto = new TicketCreateDto
        {
            Name = "Standard",
            Description = "Standard Ticket",
            Price = 100,
            Capacity = 50,
            EventId = eventId
        };

        var validationResult = new ValidationResult();
        _validatorMock.Setup(v => v.ValidateAsync(It.IsAny<TicketEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);
        _eventRepositoryMock.Setup(repo => repo.GetByIdAsync(eventId)).ReturnsAsync((Event)null);

        // Act
        var result = await _ticketService.AddTicketAsync(ticketDto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("Hata: Bilet eklemeye çalıştığınız etkinlik veritabanında bulunamadı!");
        _ticketRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<TicketEntity>()), Times.Never);
    }

    [Fact]
    public async Task AddTicketAsync_ShouldReturnSuccess_WhenEventHasNoVenue()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var ticketDto = new TicketCreateDto
        {
            Name = "Standard",
            Description = "Standard Ticket",
            Price = 100,
            Capacity = 50,
            EventId = eventId
        };

        var existingEvent = new Event { Id = eventId, Title = "Test Event", VenueId = null };
        var validationResult = new ValidationResult();

        _validatorMock.Setup(v => v.ValidateAsync(It.IsAny<TicketEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);
        _eventRepositoryMock.Setup(repo => repo.GetByIdAsync(eventId)).ReturnsAsync(existingEvent);
        _ticketRepositoryMock.Setup(repo => repo.AddAsync(It.IsAny<TicketEntity>())).Returns(Task.CompletedTask);
        _ticketRepositoryMock.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _ticketService.AddTicketAsync(ticketDto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Message.Should().Be("Bilet/Paket başarıyla oluşturuldu.");
    }

    [Fact]
    public async Task AddTicketAsync_ShouldReturnFailure_WhenCapacityExceedsVenueCapacity()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var venueId = Guid.NewGuid();
        var ticketDto = new TicketCreateDto
        {
            Name = "Standard",
            Description = "Standard Ticket",
            Price = 100,
            Capacity = 150, // Exceeds venue capacity
            EventId = eventId
        };

        var existingEvent = new Event { Id = eventId, Title = "Test Event", VenueId = venueId };
        var venue = new Venue { Id = venueId, Name = "Test Venue", Rows = 10, Columns = 10 }; // Total capacity = 100
        var validationResult = new ValidationResult();

        _validatorMock.Setup(v => v.ValidateAsync(It.IsAny<TicketEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);
        _eventRepositoryMock.Setup(repo => repo.GetByIdAsync(eventId)).ReturnsAsync(existingEvent);
        _venueRepositoryMock.Setup(repo => repo.GetByIdAsync(venueId)).ReturnsAsync(venue);
        _ticketRepositoryMock.Setup(repo => repo.GetAllAsync()).ReturnsAsync(new List<TicketEntity>());

        // Act
        var result = await _ticketService.AddTicketAsync(ticketDto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain("Hata: Sahne kapasitesi aşıldı!");
        result.Message.Should().Contain("100");
        _ticketRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<TicketEntity>()), Times.Never);
    }

    [Fact]
    public async Task AddTicketAsync_ShouldReturnFailure_WhenTotalCapacityExceedsVenueCapacity()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var venueId = Guid.NewGuid();
        var ticketDto = new TicketCreateDto
        {
            Name = "Standard",
            Description = "Standard Ticket",
            Price = 100,
            Capacity = 60, // Total would be 110 (exceeds 100)
            EventId = eventId
        };

        var existingEvent = new Event { Id = eventId, Title = "Test Event", VenueId = venueId };
        var venue = new Venue { Id = venueId, Name = "Test Venue", Rows = 10, Columns = 10 }; // Total capacity = 100
        var existingTickets = new List<TicketEntity>
        {
            new TicketEntity { Id = Guid.NewGuid(), Name = "VIP", Price = 200, Capacity = 50, EventId = eventId }
        };
        var validationResult = new ValidationResult();

        _validatorMock.Setup(v => v.ValidateAsync(It.IsAny<TicketEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);
        _eventRepositoryMock.Setup(repo => repo.GetByIdAsync(eventId)).ReturnsAsync(existingEvent);
        _venueRepositoryMock.Setup(repo => repo.GetByIdAsync(venueId)).ReturnsAsync(venue);
        _ticketRepositoryMock.Setup(repo => repo.GetAllAsync()).ReturnsAsync(existingTickets);

        // Act
        var result = await _ticketService.AddTicketAsync(ticketDto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain("Hata: Sahne kapasitesi aşıldı!");
        result.Message.Should().Contain("50"); // Current capacity
        _ticketRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<TicketEntity>()), Times.Never);
    }

    [Fact]
    public async Task AddTicketAsync_ShouldReturnSuccess_WhenCapacityFitsExactlyWithinVenueCapacity()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var venueId = Guid.NewGuid();
        var ticketDto = new TicketCreateDto
        {
            Name = "Standard",
            Description = "Standard Ticket",
            Price = 100,
            Capacity = 50,
            EventId = eventId
        };

        var existingEvent = new Event { Id = eventId, Title = "Test Event", VenueId = venueId };
        var venue = new Venue { Id = venueId, Name = "Test Venue", Rows = 10, Columns = 10 }; // Total capacity = 100
        var existingTickets = new List<TicketEntity>
        {
            new TicketEntity { Id = Guid.NewGuid(), Name = "VIP", Price = 200, Capacity = 50, EventId = eventId }
        };
        var validationResult = new ValidationResult();

        _validatorMock.Setup(v => v.ValidateAsync(It.IsAny<TicketEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);
        _eventRepositoryMock.Setup(repo => repo.GetByIdAsync(eventId)).ReturnsAsync(existingEvent);
        _venueRepositoryMock.Setup(repo => repo.GetByIdAsync(venueId)).ReturnsAsync(venue);
        _ticketRepositoryMock.Setup(repo => repo.GetAllAsync()).ReturnsAsync(existingTickets);
        _ticketRepositoryMock.Setup(repo => repo.AddAsync(It.IsAny<TicketEntity>())).Returns(Task.CompletedTask);
        _ticketRepositoryMock.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _ticketService.AddTicketAsync(ticketDto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Message.Should().Be("Bilet/Paket başarıyla oluşturuldu.");
        _ticketRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<TicketEntity>()), Times.Once);
    }

    [Fact]
    public async Task AddTicketAsync_ShouldReturnSuccess_WhenVenueExistsButIsNullForEvent()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var venueId = Guid.NewGuid();
        var ticketDto = new TicketCreateDto
        {
            Name = "Standard",
            Description = "Standard Ticket",
            Price = 100,
            Capacity = 50,
            EventId = eventId
        };

        var existingEvent = new Event { Id = eventId, Title = "Test Event", VenueId = venueId };
        var validationResult = new ValidationResult();

        _validatorMock.Setup(v => v.ValidateAsync(It.IsAny<TicketEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);
        _eventRepositoryMock.Setup(repo => repo.GetByIdAsync(eventId)).ReturnsAsync(existingEvent);
        _venueRepositoryMock.Setup(repo => repo.GetByIdAsync(venueId)).ReturnsAsync((Venue)null);
        _ticketRepositoryMock.Setup(repo => repo.GetAllAsync()).ReturnsAsync(new List<TicketEntity>());
        _ticketRepositoryMock.Setup(repo => repo.AddAsync(It.IsAny<TicketEntity>())).Returns(Task.CompletedTask);
        _ticketRepositoryMock.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _ticketService.AddTicketAsync(ticketDto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Message.Should().Be("Bilet/Paket başarıyla oluşturuldu.");
    }

    [Fact]
    public async Task AddTicketAsync_ShouldReturnFailure_WhenMultipleValidationErrorsOccur()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var ticketDto = new TicketCreateDto
        {
            Name = "Standard",
            Description = "Standard Ticket",
            Price = -50,
            Capacity = -10,
            EventId = Guid.Empty
        };

        var validationFailures = new[]
        {
            new ValidationFailure("Price", "Bilet/Paket fiyatı 0'dan küçük olamaz."),
            new ValidationFailure("Capacity", "Kapasite en az 1 olmalıdır."),
            new ValidationFailure("EventId", "Biletin bağlanacağı etkinlik ID'si boş olamaz.")
        };
        var validationResult = new ValidationResult(validationFailures);

        _validatorMock.Setup(v => v.ValidateAsync(It.IsAny<TicketEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);

        // Act
        var result = await _ticketService.AddTicketAsync(ticketDto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain("Bilet/Paket fiyatı 0'dan küçük olamaz.");
        result.Message.Should().Contain("Kapasite en az 1 olmalıdır.");
        result.Message.Should().Contain("Biletin bağlanacağı etkinlik ID'si boş olamaz.");
    }

    #endregion

    #region UpdateTicketAsync Tests

    [Fact]
    public async Task UpdateTicketAsync_ShouldReturnSuccess_WhenTicketExistsAndIsValid()
    {
        // Arrange
        var ticketId = Guid.NewGuid();
        var existingTicket = new TicketEntity
        {
            Id = ticketId,
            Name = "Standard",
            Description = "Standard Ticket",
            Price = 100,
            Capacity = 50,
            EventId = Guid.NewGuid()
        };

        var ticketUpdateDto = new TicketUpdateDto
        {
            Id = ticketId,
            Name = "Premium",
            Description = "Premium Ticket",
            Price = 150,
            Capacity = 30
        };

        var validationResult = new ValidationResult();

        _ticketRepositoryMock.Setup(repo => repo.GetByIdAsync(ticketId)).ReturnsAsync(existingTicket);
        _validatorMock.Setup(v => v.ValidateAsync(It.IsAny<TicketEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);
        _ticketRepositoryMock.Setup(repo => repo.Update(It.IsAny<TicketEntity>()));
        _ticketRepositoryMock.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _ticketService.UpdateTicketAsync(ticketUpdateDto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Message.Should().Be("Bilet başarıyla güncellendi.");
        _ticketRepositoryMock.Verify(repo => repo.Update(It.IsAny<TicketEntity>()), Times.Once);
        _ticketRepositoryMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateTicketAsync_ShouldReturnFailure_WhenTicketDoesNotExist()
    {
        // Arrange
        var ticketId = Guid.NewGuid();
        var ticketUpdateDto = new TicketUpdateDto
        {
            Id = ticketId,
            Name = "Premium",
            Description = "Premium Ticket",
            Price = 150,
            Capacity = 30
        };

        _ticketRepositoryMock.Setup(repo => repo.GetByIdAsync(ticketId)).ReturnsAsync((TicketEntity)null);

        // Act
        var result = await _ticketService.UpdateTicketAsync(ticketUpdateDto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("Güncellenecek bilet bulunamadı.");
        _ticketRepositoryMock.Verify(repo => repo.Update(It.IsAny<TicketEntity>()), Times.Never);
    }

    [Fact]
    public async Task UpdateTicketAsync_ShouldReturnFailure_WhenValidationFails()
    {
        // Arrange
        var ticketId = Guid.NewGuid();
        var existingTicket = new TicketEntity
        {
            Id = ticketId,
            Name = "Standard",
            Description = "Standard Ticket",
            Price = 100,
            Capacity = 50,
            EventId = Guid.NewGuid()
        };

        var ticketUpdateDto = new TicketUpdateDto
        {
            Id = ticketId,
            Name = "Premium",
            Description = "Premium Ticket",
            Price = -50, // Invalid
            Capacity = 30
        };

        var validationFailure = new ValidationFailure("Price", "Bilet/Paket fiyatı 0'dan küçük olamaz.");
        var validationResult = new ValidationResult(new[] { validationFailure });

        _ticketRepositoryMock.Setup(repo => repo.GetByIdAsync(ticketId)).ReturnsAsync(existingTicket);
        _validatorMock.Setup(v => v.ValidateAsync(It.IsAny<TicketEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);

        // Act
        var result = await _ticketService.UpdateTicketAsync(ticketUpdateDto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain("Bilet/Paket fiyatı 0'dan küçük olamaz.");
        _ticketRepositoryMock.Verify(repo => repo.Update(It.IsAny<TicketEntity>()), Times.Never);
    }

    [Fact]
    public async Task UpdateTicketAsync_ShouldUpdateAllProperties_WhenValid()
    {
        // Arrange
        var ticketId = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        var existingTicket = new TicketEntity
        {
            Id = ticketId,
            Name = "Standard",
            Description = "Standard Ticket",
            Price = 100,
            Capacity = 50,
            EventId = eventId
        };

        var ticketUpdateDto = new TicketUpdateDto
        {
            Id = ticketId,
            Name = "PremiumUpdated",
            Description = "Premium Ticket Updated",
            Price = 200,
            Capacity = 25
        };

        var validationResult = new ValidationResult();

        _ticketRepositoryMock.Setup(repo => repo.GetByIdAsync(ticketId)).ReturnsAsync(existingTicket);
        _validatorMock.Setup(v => v.ValidateAsync(It.IsAny<TicketEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);
        _ticketRepositoryMock.Setup(repo => repo.Update(It.IsAny<TicketEntity>()));
        _ticketRepositoryMock.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _ticketService.UpdateTicketAsync(ticketUpdateDto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _ticketRepositoryMock.Verify(repo => repo.Update(It.IsAny<TicketEntity>()), Times.Once);
    }

    #endregion

    #region DeleteTicketAsync Tests

    [Fact]
    public async Task DeleteTicketAsync_ShouldReturnSuccess_WhenTicketExists()
    {
        // Arrange
        var ticketId = Guid.NewGuid();
        var existingTicket = new TicketEntity
        {
            Id = ticketId,
            Name = "Standard",
            Price = 100,
            Capacity = 50,
            EventId = Guid.NewGuid()
        };

        _ticketRepositoryMock.Setup(repo => repo.GetByIdAsync(ticketId)).ReturnsAsync(existingTicket);
        _ticketRepositoryMock.Setup(repo => repo.Remove(It.IsAny<TicketEntity>()));
        _ticketRepositoryMock.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _ticketService.DeleteTicketAsync(ticketId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Message.Should().Be("Bilet başarıyla silindi.");
        _ticketRepositoryMock.Verify(repo => repo.Remove(It.IsAny<TicketEntity>()), Times.Once);
        _ticketRepositoryMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteTicketAsync_ShouldReturnFailure_WhenTicketDoesNotExist()
    {
        // Arrange
        var ticketId = Guid.NewGuid();
        _ticketRepositoryMock.Setup(repo => repo.GetByIdAsync(ticketId)).ReturnsAsync((TicketEntity)null);

        // Act
        var result = await _ticketService.DeleteTicketAsync(ticketId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("Silinecek bilet bulunamadı.");
        _ticketRepositoryMock.Verify(repo => repo.Remove(It.IsAny<TicketEntity>()), Times.Never);
    }

    #endregion
}
