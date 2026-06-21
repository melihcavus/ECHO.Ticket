using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ECHO.Ticket.Business.Services;
using ECHO.Ticket.Core.DTOs;
using ECHO.Ticket.Core.Entities;
using ECHO.Ticket.Core.Results;
using ECHO.Ticket.DataAccess.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace ECHO.Ticket.Tests;

public class DashboardServiceTests
{
    private readonly Mock<IRepository<Pledge>> _pledgeRepositoryMock;
    private readonly Mock<IRepository<Event>> _eventRepositoryMock;
    private readonly Mock<IRepository<Core.Entities.Ticket>> _ticketRepositoryMock;
    private readonly DashboardService _dashboardService;

    public DashboardServiceTests()
    {
        _pledgeRepositoryMock = new Mock<IRepository<Pledge>>();
        _eventRepositoryMock = new Mock<IRepository<Event>>();
        _ticketRepositoryMock = new Mock<IRepository<Core.Entities.Ticket>>();

        _dashboardService = new DashboardService(
            _pledgeRepositoryMock.Object,
            _eventRepositoryMock.Object,
            _ticketRepositoryMock.Object);
    }

    [Fact]
    public async Task GetSummaryAsync_ShouldReturnSuccess_WithCompleteData()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        var ticketId = Guid.NewGuid();

        var pledges = new List<Pledge>
        {
            new Pledge
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                TicketId = ticketId,
                AmountPaid = 100,
                PledgeDate = DateTime.UtcNow.AddDays(-2)
            },
            new Pledge
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                TicketId = ticketId,
                AmountPaid = 50,
                PledgeDate = DateTime.UtcNow.AddDays(-1)
            }
        };

        var activeEvents = new List<Event>
        {
            new Event { Id = Guid.NewGuid(), Title = "Event1", IsActive = true, EventDate = DateTime.UtcNow.AddDays(5) },
            new Event { Id = Guid.NewGuid(), Title = "Event2", IsActive = true, EventDate = DateTime.UtcNow.AddDays(10) }
        };

        var ticket = new Core.Entities.Ticket { Id = ticketId, Name = "T1", EventId = eventId };
        var eventEntity = new Event { Id = eventId, Title = "TestEvent" };

        _pledgeRepositoryMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<Pledge, bool>>>()))
            .ReturnsAsync(pledges);

        _eventRepositoryMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<Event, bool>>>()))
            .ReturnsAsync(activeEvents);

        _ticketRepositoryMock
            .Setup(r => r.GetByIdAsync(ticketId))
            .ReturnsAsync(ticket);

        _eventRepositoryMock
            .Setup(r => r.GetByIdAsync(eventId))
            .ReturnsAsync(eventEntity);

        // Act
        var result = await _dashboardService.GetSummaryAsync(userId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var summary = result.Data!;
        summary.TotalPledgeAmount.Should().Be(150); // 100 + 50
        summary.ActiveProjectCount.Should().Be(2);
        summary.UpcomingEventCount.Should().Be(2);
        summary.RecentActivities.Should().HaveCount(2);
        summary.RecentActivities[0].EventName.Should().Be("TestEvent");
        summary.RecentActivities[0].Amount.Should().Be(50);
    }

    [Fact]
    public async Task GetSummaryAsync_ShouldReturnZeroTotalPledge_WhenUserHasNoPledges()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var emptyPledges = new List<Pledge>();
        var activeEvents = new List<Event>
        {
            new Event { Id = Guid.NewGuid(), Title = "E1", IsActive = true, EventDate = DateTime.UtcNow.AddDays(5) }
        };

        _pledgeRepositoryMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<Pledge, bool>>>()))
            .ReturnsAsync(emptyPledges);

        _eventRepositoryMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<Event, bool>>>()))
            .ReturnsAsync(activeEvents);

        // Act
        var result = await _dashboardService.GetSummaryAsync(userId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var summary = result.Data!;
        summary.TotalPledgeAmount.Should().Be(0);
        summary.UpcomingEventCount.Should().Be(0);
        summary.RecentActivities.Should().BeEmpty();
    }

    [Fact]
    public async Task GetSummaryAsync_ShouldReturnZeroActiveProjects_WhenNoActiveEvents()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var ticketId = Guid.NewGuid();

        var pledges = new List<Pledge>
        {
            new Pledge
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                TicketId = ticketId,
                AmountPaid = 75,
                PledgeDate = DateTime.UtcNow
            }
        };

        var emptyActiveEvents = new List<Event>();

        var ticket = new Core.Entities.Ticket { Id = ticketId, Name = "T1", EventId = Guid.NewGuid() };
        var eventEntity = new Event { Id = Guid.NewGuid(), Title = "EvX" };

        _pledgeRepositoryMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<Pledge, bool>>>()))
            .ReturnsAsync(pledges);

        _eventRepositoryMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<Event, bool>>>()))
            .ReturnsAsync(emptyActiveEvents);

        _ticketRepositoryMock
            .Setup(r => r.GetByIdAsync(ticketId))
            .ReturnsAsync(ticket);

        _eventRepositoryMock
            .Setup(r => r.GetByIdAsync(ticket.EventId))
            .ReturnsAsync(eventEntity);

        // Act
        var result = await _dashboardService.GetSummaryAsync(userId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var summary = result.Data!;
        summary.ActiveProjectCount.Should().Be(0);
        summary.TotalPledgeAmount.Should().Be(75);
        summary.UpcomingEventCount.Should().Be(1);
    }

    [Fact]
    public async Task GetSummaryAsync_ShouldTakeLast3Pledges_WhenMoreThan3Exist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var ticketId = Guid.NewGuid();

        var pledges = new List<Pledge>
        {
            new Pledge { Id = Guid.NewGuid(), UserId = userId, TicketId = ticketId, AmountPaid = 10, PledgeDate = DateTime.UtcNow.AddDays(-5) },
            new Pledge { Id = Guid.NewGuid(), UserId = userId, TicketId = ticketId, AmountPaid = 20, PledgeDate = DateTime.UtcNow.AddDays(-4) },
            new Pledge { Id = Guid.NewGuid(), UserId = userId, TicketId = ticketId, AmountPaid = 30, PledgeDate = DateTime.UtcNow.AddDays(-3) },
            new Pledge { Id = Guid.NewGuid(), UserId = userId, TicketId = ticketId, AmountPaid = 40, PledgeDate = DateTime.UtcNow.AddDays(-2) },
            new Pledge { Id = Guid.NewGuid(), UserId = userId, TicketId = ticketId, AmountPaid = 50, PledgeDate = DateTime.UtcNow.AddDays(-1) }
        };

        var activeEvents = new List<Event>();
        var ticket = new Core.Entities.Ticket { Id = ticketId, Name = "T1", EventId = Guid.NewGuid() };
        var eventEntity = new Event { Id = Guid.NewGuid(), Title = "Event" };

        _pledgeRepositoryMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<Pledge, bool>>>()))
            .ReturnsAsync(pledges);

        _eventRepositoryMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<Event, bool>>>()))
            .ReturnsAsync(activeEvents);

        _ticketRepositoryMock
            .Setup(r => r.GetByIdAsync(ticketId))
            .ReturnsAsync(ticket);

        _eventRepositoryMock
            .Setup(r => r.GetByIdAsync(ticket.EventId))
            .ReturnsAsync(eventEntity);

        // Act
        var result = await _dashboardService.GetSummaryAsync(userId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var summary = result.Data!;
        summary.RecentActivities.Should().HaveCount(3);
        // Most recent 3: 50, 40, 30
        summary.RecentActivities[0].Amount.Should().Be(50);
        summary.RecentActivities[1].Amount.Should().Be(40);
        summary.RecentActivities[2].Amount.Should().Be(30);
    }

    [Fact]
    public async Task GetSummaryAsync_ShouldHandleTicketNull_WhenTicketNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var ticketId = Guid.NewGuid();

        var pledges = new List<Pledge>
        {
            new Pledge
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                TicketId = ticketId,
                AmountPaid = 50,
                PledgeDate = DateTime.UtcNow
            }
        };

        var activeEvents = new List<Event>();

        _pledgeRepositoryMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<Pledge, bool>>>()))
            .ReturnsAsync(pledges);

        _eventRepositoryMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<Event, bool>>>()))
            .ReturnsAsync(activeEvents);

        _ticketRepositoryMock
            .Setup(r => r.GetByIdAsync(ticketId))
            .ReturnsAsync((Core.Entities.Ticket)null);

        // Act
        var result = await _dashboardService.GetSummaryAsync(userId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var summary = result.Data!;
        summary.RecentActivities.Should().HaveCount(1);
        summary.RecentActivities[0].EventName.Should().Be("Bilinmeyen Etkinlik");
    }

    [Fact]
    public async Task GetSummaryAsync_ShouldShowUnknownEvent_WhenEventNull()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var ticketId = Guid.NewGuid();
        var eventId = Guid.NewGuid();

        var pledges = new List<Pledge>
        {
            new Pledge
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                TicketId = ticketId,
                AmountPaid = 123,
                PledgeDate = DateTime.UtcNow
            }
        };

        var activeEvents = new List<Event>();
        var ticket = new Core.Entities.Ticket { Id = ticketId, Name = "T1", EventId = eventId };

        _pledgeRepositoryMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<Pledge, bool>>>()))
            .ReturnsAsync(pledges);

        _eventRepositoryMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<Event, bool>>>()))
            .ReturnsAsync(activeEvents);

        _ticketRepositoryMock
            .Setup(r => r.GetByIdAsync(ticketId))
            .ReturnsAsync(ticket);

        _eventRepositoryMock
            .Setup(r => r.GetByIdAsync(eventId))
            .ReturnsAsync((Event)null);

        // Act
        var result = await _dashboardService.GetSummaryAsync(userId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var summary = result.Data!;
        summary.RecentActivities.Should().HaveCount(1);
        summary.RecentActivities[0].EventName.Should().Be("Bilinmeyen Etkinlik");
        summary.RecentActivities[0].Amount.Should().Be(123);
        summary.RecentActivities[0].Type.Should().Be("Bilet / Destek");
    }

    [Fact]
    public async Task GetSummaryAsync_ShouldReturnFailure_WhenPledgeRepositoryThrowsException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _pledgeRepositoryMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<Pledge, bool>>>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _dashboardService.GetSummaryAsync(userId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain("Dashboard verileri alınırken bir hata oluştu: Database error");
    }

    [Fact]
    public async Task GetSummaryAsync_ShouldReturnFailure_WhenEventRepositoryThrowsException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var pledges = new List<Pledge>
        {
            new Pledge { Id = Guid.NewGuid(), UserId = userId, TicketId = Guid.NewGuid(), AmountPaid = 50, PledgeDate = DateTime.UtcNow }
        };

        _pledgeRepositoryMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<Pledge, bool>>>()))
            .ReturnsAsync(pledges);

        _eventRepositoryMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<Event, bool>>>()))
            .ThrowsAsync(new Exception("Event fetch boom"));

        // Act
        var result = await _dashboardService.GetSummaryAsync(userId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain("Dashboard verileri alınırken bir hata oluştu: Event fetch boom");
    }

    [Fact]
    public async Task GetSummaryAsync_ShouldReturnFailure_WhenTicketRepositoryThrowsException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var ticketId = Guid.NewGuid();

        var pledges = new List<Pledge>
        {
            new Pledge
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                TicketId = ticketId,
                AmountPaid = 50,
                PledgeDate = DateTime.UtcNow
            }
        };

        var activeEvents = new List<Event>();

        _pledgeRepositoryMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<Pledge, bool>>>()))
            .ReturnsAsync(pledges);

        _eventRepositoryMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<Event, bool>>>()))
            .ReturnsAsync(activeEvents);

        _ticketRepositoryMock
            .Setup(r => r.GetByIdAsync(ticketId))
            .ThrowsAsync(new Exception("Ticket fetch error"));

        // Act
        var result = await _dashboardService.GetSummaryAsync(userId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain("Dashboard verileri alınırken bir hata oluştu: Ticket fetch error");
    }

    [Fact]
    public async Task GetSummaryAsync_ShouldCorrectlyCalculateTotalPledgeAmount_WithMultiplePledges()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var ticketId = Guid.NewGuid();

        var pledges = new List<Pledge>
        {
            new Pledge { Id = Guid.NewGuid(), UserId = userId, TicketId = ticketId, AmountPaid = 100.50m, PledgeDate = DateTime.UtcNow.AddDays(-1) },
            new Pledge { Id = Guid.NewGuid(), UserId = userId, TicketId = ticketId, AmountPaid = 250.75m, PledgeDate = DateTime.UtcNow },
            new Pledge { Id = Guid.NewGuid(), UserId = userId, TicketId = ticketId, AmountPaid = 49.25m, PledgeDate = DateTime.UtcNow.AddDays(-2) }
        };

        var activeEvents = new List<Event>();
        var ticket = new Core.Entities.Ticket { Id = ticketId, Name = "T1", EventId = Guid.NewGuid() };
        var eventEntity = new Event { Id = Guid.NewGuid(), Title = "E1" };

        _pledgeRepositoryMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<Pledge, bool>>>()))
            .ReturnsAsync(pledges);

        _eventRepositoryMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<Event, bool>>>()))
            .ReturnsAsync(activeEvents);

        _ticketRepositoryMock
            .Setup(r => r.GetByIdAsync(ticketId))
            .ReturnsAsync(ticket);

        _eventRepositoryMock
            .Setup(r => r.GetByIdAsync(ticket.EventId))
            .ReturnsAsync(eventEntity);

        // Act
        var result = await _dashboardService.GetSummaryAsync(userId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var summary = result.Data!;
        summary.TotalPledgeAmount.Should().Be(400.50m); // 100.50 + 250.75 + 49.25
    }

    [Fact]
    public async Task GetSummaryAsync_ShouldPreserveRecentActivityOrder_ByPledgeDate()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var ticketId = Guid.NewGuid();

        var pledges = new List<Pledge>
        {
            new Pledge { Id = Guid.NewGuid(), UserId = userId, TicketId = ticketId, AmountPaid = 10, PledgeDate = DateTime.UtcNow.AddDays(-3) },
            new Pledge { Id = Guid.NewGuid(), UserId = userId, TicketId = ticketId, AmountPaid = 20, PledgeDate = DateTime.UtcNow.AddDays(-1) },
            new Pledge { Id = Guid.NewGuid(), UserId = userId, TicketId = ticketId, AmountPaid = 30, PledgeDate = DateTime.UtcNow.AddDays(-2) }
        };

        var activeEvents = new List<Event>();
        var ticket = new Core.Entities.Ticket { Id = ticketId, Name = "T1", EventId = Guid.NewGuid() };
        var eventEntity = new Event { Id = Guid.NewGuid(), Title = "E1" };

        _pledgeRepositoryMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<Pledge, bool>>>()))
            .ReturnsAsync(pledges);

        _eventRepositoryMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<Event, bool>>>()))
            .ReturnsAsync(activeEvents);

        _ticketRepositoryMock
            .Setup(r => r.GetByIdAsync(ticketId))
            .ReturnsAsync(ticket);

        _eventRepositoryMock
            .Setup(r => r.GetByIdAsync(ticket.EventId))
            .ReturnsAsync(eventEntity);

        // Act
        var result = await _dashboardService.GetSummaryAsync(userId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var summary = result.Data!;
        summary.RecentActivities.Should().HaveCount(3);
        // Should be ordered by PledgeDate descending: 20 (latest), 30, 10
        summary.RecentActivities[0].Amount.Should().Be(20);
        summary.RecentActivities[1].Amount.Should().Be(30);
        summary.RecentActivities[2].Amount.Should().Be(10);
    }
}
