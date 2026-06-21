using ECHO.Ticket.Business.Services;
using ECHO.Ticket.Business.Interfaces;
using ECHO.Ticket.Core.DTOs;
using ECHO.Ticket.Core.Entities;
using ECHO.Ticket.Core.Results;
using ECHO.Ticket.DataAccess.Interfaces;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using Xunit;
using PledgeEntity = ECHO.Ticket.Core.Entities.Pledge;
using UserEntity = ECHO.Ticket.Core.Entities.User;
using TicketEntity = ECHO.Ticket.Core.Entities.Ticket;

namespace ECHO.Ticket.Tests;

public class PledgeServiceTests
{
    private readonly Mock<IRepository<PledgeEntity>> _pledgeRepositoryMock;
    private readonly Mock<IRepository<UserEntity>> _userRepositoryMock;
    private readonly Mock<IRepository<TicketEntity>> _ticketRepositoryMock;
    private readonly Mock<IRepository<Event>> _eventRepositoryMock;
    private readonly Mock<IValidator<PledgeEntity>> _validatorMock;
    private readonly Mock<IWorkContext> _workContextMock;
    private readonly PledgeService _pledgeService;

    public PledgeServiceTests()
    {
        _pledgeRepositoryMock = new Mock<IRepository<PledgeEntity>>();
        _userRepositoryMock = new Mock<IRepository<UserEntity>>();
        _ticketRepositoryMock = new Mock<IRepository<TicketEntity>>();
        _eventRepositoryMock = new Mock<IRepository<Event>>();
        _validatorMock = new Mock<IValidator<PledgeEntity>>();
        _workContextMock = new Mock<IWorkContext>();

        _pledgeService = new PledgeService(
            _pledgeRepositoryMock.Object,
            _userRepositoryMock.Object,
            _ticketRepositoryMock.Object,
            _eventRepositoryMock.Object,
            _validatorMock.Object,
            _workContextMock.Object);
    }

    [Fact]
    public async Task GetAllPledgesAsync_ShouldReturnSuccess_WithPledges()
    {
        // Arrange
        var pledges = new List<PledgeEntity>
        {
            new PledgeEntity { Id = Guid.NewGuid(), AmountPaid = 10, UserId = Guid.NewGuid(), TicketId = Guid.NewGuid() },
            new PledgeEntity { Id = Guid.NewGuid(), AmountPaid = 20, UserId = Guid.NewGuid(), TicketId = Guid.NewGuid() }
        };
        _pledgeRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(pledges);

        // Act
        var result = await _pledgeService.GetAllPledgesAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeEquivalentTo(pledges);
    }

    [Fact]
    public async Task GetPledgeByIdAsync_ShouldReturnSuccess_WhenPledgeExists()
    {
        // Arrange
        var id = Guid.NewGuid();
        var pledge = new PledgeEntity { Id = id, AmountPaid = 50 };
        _pledgeRepositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(pledge);

        // Act
        var result = await _pledgeService.GetPledgeByIdAsync(id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().Be(pledge);
    }

    [Fact]
    public async Task GetPledgeByIdAsync_ShouldReturnFailure_WhenPledgeNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        _pledgeRepositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((PledgeEntity)null);

        // Act
        var result = await _pledgeService.GetPledgeByIdAsync(id);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("Belirtilen ID'ye sahip bir destek (Pledge) bulunamadı.");
    }

    [Fact]
    public async Task GetPledgesByUserIdAsync_ShouldReturnFailure_WhenUserHasNoPledges()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _pledgeRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<PledgeEntity>());

        // Act
        var result = await _pledgeService.GetPledgesByUserIdAsync(userId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("Henüz bir biletin bulunmuyor.");
    }

    [Fact]
    public async Task GetPledgesByUserIdAsync_ShouldReturnSuccess_WhenPledgesExistWithEventAndTicket()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var ticketId = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        var pledge = new PledgeEntity
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TicketId = ticketId,
            AmountPaid = 123,
            PledgeDate = DateTime.UtcNow.AddDays(-1)
        };

        _pledgeRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<PledgeEntity> { pledge });

        var ticket = new TicketEntity { Id = ticketId, Name = "VIP", Price = 123, Capacity = 10, EventId = eventId };
        _ticketRepositoryMock.Setup(r => r.GetByIdAsync(ticketId)).ReturnsAsync(ticket);

        var eventEntity = new Event { Id = eventId, Title = "My Event" };
        _eventRepositoryMock.Setup(r => r.GetByIdAsync(eventId)).ReturnsAsync(eventEntity);

        // Act
        var result = await _pledgeService.GetPledgesByUserIdAsync(userId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var list = result.Data!.ToList();
        list.Should().HaveCount(1);
        list[0].PledgeId.Should().Be(pledge.Id);
        list[0].TicketName.Should().Be("VIP");
        list[0].EventTitle.Should().Be("My Event");
        list[0].AmountPaid.Should().Be(123);
    }

    [Fact]
    public async Task GetPledgesByUserIdAsync_ShouldReturnEventDeletedLabel_WhenEventIsNull()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var ticketId = Guid.NewGuid();
        var pledge = new PledgeEntity
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TicketId = ticketId,
            AmountPaid = 50,
            PledgeDate = DateTime.UtcNow
        };

        _pledgeRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<PledgeEntity> { pledge });

        var ticket = new TicketEntity { Id = ticketId, Name = "Standard", Price = 50, EventId = Guid.NewGuid() };
        _ticketRepositoryMock.Setup(r => r.GetByIdAsync(ticketId)).ReturnsAsync(ticket);

        _eventRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Event)null);

        // Act
        var result = await _pledgeService.GetPledgesByUserIdAsync(userId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var list = result.Data!.ToList();
        list.Should().HaveCount(1);
        list[0].EventTitle.Should().Be("Etkinlik Silinmiş");
        list[0].TicketName.Should().Be("Standard");
    }

    [Fact]
    public async Task AddPledgeAsync_ShouldReturnSuccess_WhenAllChecksPass()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var ticketId = Guid.NewGuid();
        var pledgeDto = new PledgeCreateDto { UserId = Guid.NewGuid(), TicketId = ticketId };

        _workContextMock.SetupGet(w => w.UserId).Returns(userId);

        var user = new UserEntity { Id = userId, Email = "a@b.com" };
        _userRepositoryMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);

        var ticket = new TicketEntity { Id = ticketId, Name = "T1", Price = 75, Capacity = 10, EventId = Guid.NewGuid() };
        _ticketRepositoryMock.Setup(r => r.GetByIdAsync(ticketId)).ReturnsAsync(ticket);

        _validatorMock.Setup(v => v.ValidateAsync(It.IsAny<PledgeEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        PledgeEntity captured = null!;
        _pledgeRepositoryMock.Setup(r => r.AddAsync(It.IsAny<PledgeEntity>()))
            .Callback<PledgeEntity>(p => captured = p)
            .Returns(Task.CompletedTask);
        _pledgeRepositoryMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1); // HATA BURADA DÜZELTİLDİ

        // Act
        var result = await _pledgeService.AddPledgeAsync(pledgeDto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Message.Should().Be("Satın alma işlemi başarıyla tamamlandı.");
        captured.Should().NotBeNull();
        captured.UserId.Should().Be(userId);
        captured.AmountPaid.Should().Be(ticket.Price);
        captured.PledgeDate.Should().BeCloseTo(DateTime.UtcNow, precision: TimeSpan.FromSeconds(5));
        _pledgeRepositoryMock.Verify(r => r.AddAsync(It.IsAny<PledgeEntity>()), Times.Once);
        _pledgeRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task AddPledgeAsync_ShouldReturnFailure_WhenUserNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var ticketId = Guid.NewGuid();
        var pledgeDto = new PledgeCreateDto { UserId = Guid.NewGuid(), TicketId = ticketId };

        _workContextMock.SetupGet(w => w.UserId).Returns(userId);
        _userRepositoryMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync((UserEntity)null);

        // Act
        var result = await _pledgeService.AddPledgeAsync(pledgeDto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("Hata: Destek yapmak isteyen kullanıcı sistemde bulunamadı!");
        _pledgeRepositoryMock.Verify(r => r.AddAsync(It.IsAny<PledgeEntity>()), Times.Never);
    }

    [Fact]
    public async Task AddPledgeAsync_ShouldReturnFailure_WhenTicketNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var ticketId = Guid.NewGuid();
        var pledgeDto = new PledgeCreateDto { UserId = Guid.NewGuid(), TicketId = ticketId };

        _workContextMock.SetupGet(w => w.UserId).Returns(userId);
        var user = new UserEntity { Id = userId };
        _userRepositoryMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);

        _ticketRepositoryMock.Setup(r => r.GetByIdAsync(ticketId)).ReturnsAsync((TicketEntity)null); // HATA BURADA DÜZELTİLDİ

        // Act
        var result = await _pledgeService.AddPledgeAsync(pledgeDto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("Hata: Satın alınmak istenen bilet/paket sistemde bulunamadı!");
        _pledgeRepositoryMock.Verify(r => r.AddAsync(It.IsAny<PledgeEntity>()), Times.Never);
    }

    [Fact]
    public async Task AddPledgeAsync_ShouldReturnFailure_WhenValidationFails()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var ticketId = Guid.NewGuid();
        var pledgeDto = new PledgeCreateDto { UserId = Guid.NewGuid(), TicketId = ticketId };

        _workContextMock.SetupGet(w => w.UserId).Returns(userId);
        var user = new UserEntity { Id = userId };
        _userRepositoryMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);

        var ticket = new TicketEntity { Id = ticketId, Price = 10, EventId = Guid.NewGuid() };
        _ticketRepositoryMock.Setup(r => r.GetByIdAsync(ticketId)).ReturnsAsync(ticket);

        var failure = new ValidationFailure("X", "Invalid pledge");
        var validationResult = new ValidationResult(new[] { failure });
        _validatorMock.Setup(v => v.ValidateAsync(It.IsAny<PledgeEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);

        // Act
        var result = await _pledgeService.AddPledgeAsync(pledgeDto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain("Invalid pledge");
        _pledgeRepositoryMock.Verify(r => r.AddAsync(It.IsAny<PledgeEntity>()), Times.Never);
    }

    [Fact]
    public async Task DeletePledgeAsync_ShouldReturnSuccess_WhenPledgeExists()
    {
        // Arrange
        var id = Guid.NewGuid();
        var pledge = new PledgeEntity { Id = id };
        _pledgeRepositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(pledge);
        _pledgeRepositoryMock.Setup(r => r.Remove(It.IsAny<PledgeEntity>()));
        _pledgeRepositoryMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1); // HATA BURADA DÜZELTİLDİ

        // Act
        var result = await _pledgeService.DeletePledgeAsync(id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Message.Should().Be("Destek başarıyla iptal edildi.");
        _pledgeRepositoryMock.Verify(r => r.Remove(It.IsAny<PledgeEntity>()), Times.Once);
        _pledgeRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeletePledgeAsync_ShouldReturnFailure_WhenPledgeDoesNotExist()
    {
        // Arrange
        var id = Guid.NewGuid();
        _pledgeRepositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((PledgeEntity)null);

        // Act
        var result = await _pledgeService.DeletePledgeAsync(id);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("İptal edilecek destek (Pledge) bulunamadı.");
        _pledgeRepositoryMock.Verify(r => r.Remove(It.IsAny<PledgeEntity>()), Times.Never);
    }
}