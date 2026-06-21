using System;
using ECHO.Ticket.Business.Validations;
using ECHO.Ticket.Core.Entities;
using FluentAssertions;
using FluentValidation.TestHelper;
using Xunit;
using UserEntity = ECHO.Ticket.Core.Entities.User;
using TicketEntity = ECHO.Ticket.Core.Entities.Ticket;
using PledgeEntity = ECHO.Ticket.Core.Entities.Pledge;

namespace ECHO.Ticket.Tests;

public class TicketValidatorTests
{
    private readonly TicketValidator _validator = new();

    [Fact]
    public void TicketValidator_ShouldPass_WhenAllFieldsValid()
    {
        // Arrange
        var ticket = new TicketEntity
        {
            Price = 100,
            EventId = Guid.NewGuid(),
            Capacity = 50
        };

        // Act & Assert
        _validator.TestValidate(ticket).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void TicketValidator_ShouldFail_WhenPriceNegative()
    {
        // Arrange
        var ticket = new TicketEntity
        {
            Price = -50,
            EventId = Guid.NewGuid(),
            Capacity = 50
        };

        // Act & Assert
        _validator.TestValidate(ticket)
            .ShouldHaveValidationErrorFor(t => t.Price)
            .WithErrorMessage("Bilet/Paket fiyatı 0'dan küçük olamaz.");
    }

    [Fact]
    public void TicketValidator_ShouldPass_WhenPriceIsZero()
    {
        // Arrange
        var ticket = new TicketEntity
        {
            Price = 0,
            EventId = Guid.NewGuid(),
            Capacity = 1
        };

        // Act & Assert
        _validator.TestValidate(ticket).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void TicketValidator_ShouldFail_WhenEventIdEmpty()
    {
        // Arrange
        var ticket = new TicketEntity
        {
            Price = 100,
            EventId = Guid.Empty,
            Capacity = 50
        };

        // Act & Assert
        _validator.TestValidate(ticket)
            .ShouldHaveValidationErrorFor(t => t.EventId)
            .WithErrorMessage("Biletin bağlanacağı etkinlik ID'si boş olamaz.");
    }

    [Fact]
    public void TicketValidator_ShouldFail_WhenCapacityZero()
    {
        // Arrange
        var ticket = new TicketEntity
        {
            Price = 100,
            EventId = Guid.NewGuid(),
            Capacity = 0
        };

        // Act & Assert
        _validator.TestValidate(ticket)
            .ShouldHaveValidationErrorFor(t => t.Capacity)
            .WithErrorMessage("Kapasite en az 1 olmalıdır.");
    }

    [Fact]
    public void TicketValidator_ShouldFail_WhenCapacityNegative()
    {
        // Arrange
        var ticket = new TicketEntity
        {
            Price = 100,
            EventId = Guid.NewGuid(),
            Capacity = -10
        };

        // Act & Assert
        _validator.TestValidate(ticket)
            .ShouldHaveValidationErrorFor(t => t.Capacity)
            .WithErrorMessage("Kapasite en az 1 olmalıdır.");
    }

    [Fact]
    public void TicketValidator_ShouldPass_WhenCapacityIsOne()
    {
        // Arrange
        var ticket = new TicketEntity
        {
            Price = 100,
            EventId = Guid.NewGuid(),
            Capacity = 1
        };

        // Act & Assert
        _validator.TestValidate(ticket).ShouldNotHaveAnyValidationErrors();
    }
}

public class EventValidatorTests
{
    private readonly EventValidator _validator = new();

    [Fact]
    public void EventValidator_ShouldPass_WhenAllFieldsValid()
    {
        // Arrange
        var ev = new Event
        {
            Title = "Concert",
            EventDate = DateTime.UtcNow.AddDays(5)
        };

        // Act & Assert
        _validator.TestValidate(ev).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void EventValidator_ShouldFail_WhenTitleEmpty()
    {
        // Arrange
        var ev = new Event
        {
            Title = "",
            EventDate = DateTime.UtcNow.AddDays(5)
        };

        // Act & Assert
        _validator.TestValidate(ev)
            .ShouldHaveValidationErrorFor(e => e.Title)
            .WithErrorMessage("Etkinlik adı boş bırakılamaz.");
    }

    [Fact]
    public void EventValidator_ShouldFail_WhenTitleTooShort()
    {
        // Arrange
        var ev = new Event
        {
            Title = "AB",
            EventDate = DateTime.UtcNow.AddDays(5)
        };

        // Act & Assert
        _validator.TestValidate(ev)
            .ShouldHaveValidationErrorFor(e => e.Title)
            .WithErrorMessage("Etkinlik adı en az 3 karakter olmalıdır.");
    }

    [Fact]
    public void EventValidator_ShouldPass_WhenTitleHas3Characters()
    {
        // Arrange
        var ev = new Event
        {
            Title = "ABC",
            EventDate = DateTime.UtcNow.AddDays(1)
        };

        // Act & Assert
        _validator.TestValidate(ev).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void EventValidator_ShouldFail_WhenEventDateInPast()
    {
        // Arrange
        var ev = new Event
        {
            Title = "Past Event",
            EventDate = DateTime.UtcNow.AddDays(-1)
        };

        // Act & Assert
        _validator.TestValidate(ev)
            .ShouldHaveValidationErrorFor(e => e.EventDate)
            .WithErrorMessage("Etkinlik tarihi geçmiş bir tarih olamaz.");
    }

    [Fact]
    public void EventValidator_ShouldPass_WhenEventDateInFuture()
    {
        // Arrange
        var ev = new Event
        {
            Title = "Future Event",
            EventDate = DateTime.UtcNow.AddDays(100)
        };

        // Act & Assert
        _validator.TestValidate(ev).ShouldNotHaveAnyValidationErrors();
    }
}

public class PledgeValidatorTests
{
    private readonly PledgeValidator _validator = new();

    [Fact]
    public void PledgeValidator_ShouldPass_WhenAllFieldsValid()
    {
        // Arrange
        var pledge = new PledgeEntity
        {
            UserId = Guid.NewGuid(),
            TicketId = Guid.NewGuid()
        };

        // Act & Assert
        _validator.TestValidate(pledge).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void PledgeValidator_ShouldFail_WhenUserIdEmpty()
    {
        // Arrange
        var pledge = new PledgeEntity
        {
            UserId = Guid.Empty,
            TicketId = Guid.NewGuid()
        };

        // Act & Assert
        _validator.TestValidate(pledge)
            .ShouldHaveValidationErrorFor(p => p.UserId)
            .WithErrorMessage("Destek yapan kullanıcı ID'si boş olamaz.");
    }

    [Fact]
    public void PledgeValidator_ShouldFail_WhenTicketIdEmpty()
    {
        // Arrange
        var pledge = new PledgeEntity
        {
            UserId = Guid.NewGuid(),
            TicketId = Guid.Empty
        };

        // Act & Assert
        _validator.TestValidate(pledge)
            .ShouldHaveValidationErrorFor(p => p.TicketId)
            .WithErrorMessage("Satın alınan bilet ID'si boş olamaz.");
    }

    [Fact]
    public void PledgeValidator_ShouldFail_WhenBothIdsEmpty()
    {
        // Arrange
        var pledge = new PledgeEntity
        {
            UserId = Guid.Empty,
            TicketId = Guid.Empty
        };

        // Act & Assert
        var result = _validator.TestValidate(pledge);
        result.ShouldHaveValidationErrorFor(p => p.UserId);
        result.ShouldHaveValidationErrorFor(p => p.TicketId);
    }
}

public class UserValidatorTests
{
    private readonly UserValidator _validator = new();

    [Fact]
    public void UserValidator_ShouldPass_WhenEmailValid()
    {
        // Arrange
        var user = new UserEntity
        {
            Email = "test@example.com"
        };

        // Act & Assert
        _validator.TestValidate(user).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void UserValidator_ShouldFail_WhenEmailEmpty()
    {
        // Arrange
        var user = new UserEntity
        {
            Email = ""
        };

        // Act & Assert
        _validator.TestValidate(user)
            .ShouldHaveValidationErrorFor(u => u.Email)
            .WithErrorMessage("E-posta adresi boş bırakılamaz.");
    }

    [Fact]
    public void UserValidator_ShouldFail_WhenEmailInvalid()
    {
        // Arrange
        var user = new UserEntity
        {
            Email = "notanemail"
        };

        // Act & Assert
        _validator.TestValidate(user)
            .ShouldHaveValidationErrorFor(u => u.Email)
            .WithErrorMessage("Lütfen geçerli bir e-posta adresi giriniz.");
    }

    [Fact]
    public void UserValidator_ShouldPass_WithMultipleDomainEmail()
    {
        // Arrange
        var user = new UserEntity
        {
            Email = "user.name+tag@example.co.uk"
        };

        // Act & Assert
        _validator.TestValidate(user).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void UserValidator_ShouldFail_WhenEmailMissingAtSymbol()
    {
        // Arrange
        var user = new UserEntity
        {
            Email = "testexample.com"
        };

        // Act & Assert
        _validator.TestValidate(user)
            .ShouldHaveValidationErrorFor(u => u.Email);
    }

    [Fact]
    public void UserValidator_ShouldFail_WhenEmailMissingDomain()
    {
        // Arrange
        var user = new UserEntity
        {
            Email = "test@"
        };

        // Act & Assert
        _validator.TestValidate(user)
            .ShouldHaveValidationErrorFor(u => u.Email);
    }
}
