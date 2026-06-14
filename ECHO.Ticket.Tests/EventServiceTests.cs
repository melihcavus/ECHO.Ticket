using ECHO.Ticket.Business.Services;
using ECHO.Ticket.Core.Entities;
using ECHO.Ticket.Core.Results;
using ECHO.Ticket.DataAccess.Interfaces;
using FluentAssertions;
using FluentValidation;
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

    [Fact]
    public async Task GetEventByIdAsync_ShouldReturnSuccess_WhenEventExists()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var expectedEvent = new Event { Id = eventId, Title = "Test Event" };
        _eventRepositoryMock.Setup(repo => repo.GetByIdAsync(eventId)).ReturnsAsync(expectedEvent);

        // Act
        var result = await _eventService.GetEventByIdAsync(eventId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Should().Be(expectedEvent);
        result.Message.Should().BeEmpty();
    }

    [Fact]
    public async Task GetEventByIdAsync_ShouldReturnFailure_WhenEventDoesNotExist()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        _eventRepositoryMock.Setup(repo => repo.GetByIdAsync(eventId)).ReturnsAsync((Event)null);

        // Act
        var result = await _eventService.GetEventByIdAsync(eventId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Data.Should().BeNull();
        result.Message.Should().Be("Belirtilen ID'ye sahip bir etkinlik bulunamadı.");
    }
}