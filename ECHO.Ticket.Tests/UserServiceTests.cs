using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
using BCrypt.Net;
using UserEntity = ECHO.Ticket.Core.Entities.User;

namespace ECHO.Ticket.Tests;

public class UserServiceTests
{
    private readonly Mock<IRepository<UserEntity>> _userRepositoryMock;
    private readonly Mock<IValidator<UserEntity>> _validatorMock;
    private readonly Mock<IJwtProvider> _jwtProviderMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<IWorkContext> _workContextMock;
    private readonly UserService _userService;

    public UserServiceTests()
    {
        _userRepositoryMock = new Mock<IRepository<UserEntity>>();
        _validatorMock = new Mock<IValidator<UserEntity>>();
        _jwtProviderMock = new Mock<IJwtProvider>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _workContextMock = new Mock<IWorkContext>();

        _userService = new UserService(
            _userRepositoryMock.Object,
            _validatorMock.Object,
            _jwtProviderMock.Object,
            _passwordHasherMock.Object,
            _workContextMock.Object);
    }

    #region LoginAsync Tests

    [Fact]
    public async Task LoginAsync_ShouldReturnSuccess_WhenCredentialsAreValid()
    {
        // Arrange
        var email = "test@example.com";
        var password = "correctpassword";
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
        var user = new UserEntity { Id = Guid.NewGuid(), Email = email, PasswordHash = hashedPassword };
        var loginDto = new UserLoginDto { Email = email, Password = password };
        var expectedToken = "mocked-jwt-token";

        _userRepositoryMock.Setup(repo => repo.GetAllAsync()).ReturnsAsync(new List<UserEntity> { user });
        _jwtProviderMock.Setup(provider => provider.GenerateToken(user)).Returns(expectedToken);

        // Act
        var result = await _userService.LoginAsync(loginDto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().Be(expectedToken);
        result.Message.Should().Be("Giriş başarılı! Token üretildi.");
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnFailure_WhenUserNotFound()
    {
        // Arrange
        var loginDto = new UserLoginDto { Email = "nonexistent@example.com", Password = "password" };
        _userRepositoryMock.Setup(repo => repo.GetAllAsync()).ReturnsAsync(new List<UserEntity>());

        // Act
        var result = await _userService.LoginAsync(loginDto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Data.Should().BeNull();
        result.Message.Should().Be("Hata: Bu e-posta adresiyle kayıtlı bir kullanıcı bulunamadı.");
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnFailure_WhenPasswordIsIncorrect()
    {
        // Arrange
        var email = "test@example.com";
        var correctPassword = "correctpassword";
        var wrongPassword = "wrongpassword";
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(correctPassword);
        var user = new UserEntity { Id = Guid.NewGuid(), Email = email, PasswordHash = hashedPassword };
        var loginDto = new UserLoginDto { Email = email, Password = wrongPassword };

        _userRepositoryMock.Setup(repo => repo.GetAllAsync()).ReturnsAsync(new List<UserEntity> { user });

        // Act
        var result = await _userService.LoginAsync(loginDto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Data.Should().BeNull();
        result.Message.Should().Be("Hata: Girdiğiniz şifre yanlış.");
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnFailure_WithMultipleUsers_WhenEmailDoesNotMatch()
    {
        // Arrange
        var users = new List<UserEntity>
        {
            new UserEntity { Id = Guid.NewGuid(), Email = "user1@test.com", PasswordHash = "hash1" },
            new UserEntity { Id = Guid.NewGuid(), Email = "user2@test.com", PasswordHash = "hash2" }
        };
        var loginDto = new UserLoginDto { Email = "user3@test.com", Password = "password" };

        _userRepositoryMock.Setup(repo => repo.GetAllAsync()).ReturnsAsync(users);

        // Act
        var result = await _userService.LoginAsync(loginDto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("Hata: Bu e-posta adresiyle kayıtlı bir kullanıcı bulunamadı.");
    }

    #endregion

    #region GetAllUsersAsync Tests

    [Fact]
    public async Task GetAllUsersAsync_ShouldReturnSuccess_WithUsers()
    {
        // Arrange
        var users = new List<UserEntity>
        {
            new UserEntity { Id = Guid.NewGuid(), Email = "user1@test.com" },
            new UserEntity { Id = Guid.NewGuid(), Email = "user2@test.com" }
        };
        _userRepositoryMock.Setup(repo => repo.GetAllAsync()).ReturnsAsync(users);

        // Act
        var result = await _userService.GetAllUsersAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().HaveCount(2);
        result.Data.Should().BeEquivalentTo(users);
    }

    [Fact]
    public async Task GetAllUsersAsync_ShouldReturnSuccess_WithEmptyList()
    {
        // Arrange
        var emptyUsers = new List<UserEntity>();
        _userRepositoryMock.Setup(repo => repo.GetAllAsync()).ReturnsAsync(emptyUsers);

        // Act
        var result = await _userService.GetAllUsersAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeEmpty();
    }

    #endregion

    #region GetUserByIdAsync Tests

    [Fact]
    public async Task GetUserByIdAsync_ShouldReturnSuccess_WhenUserExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new UserEntity { Id = userId, Email = "test@test.com" };
        _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync(user);

        // Act
        var result = await _userService.GetUserByIdAsync(userId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().Be(user);
    }

    [Fact]
    public async Task GetUserByIdAsync_ShouldReturnFailure_WhenUserNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync((UserEntity)null);

        // Act
        var result = await _userService.GetUserByIdAsync(userId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("Kullanıcı bulunamadı.");
    }

    #endregion

    #region AddUserAsync Tests

    [Fact]
    public async Task AddUserAsync_ShouldReturnSuccess_WhenValidationPassesAndEmailIsUnique()
    {
        // Arrange
        var dto = new UserCreateDto
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@test.com",
            PasswordHash = "plainpassword",
            Location = "Istanbul"
        };

        var hashedPassword = "hashedpassword123";
        _passwordHasherMock.Setup(ph => ph.HashPassword("plainpassword")).Returns(hashedPassword);

        _validatorMock.Setup(v => v.ValidateAsync(It.IsAny<UserEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _userRepositoryMock.Setup(repo => repo.GetAllAsync()).ReturnsAsync(new List<UserEntity>());
        _userRepositoryMock.Setup(repo => repo.AddAsync(It.IsAny<UserEntity>())).Returns(Task.CompletedTask);
        _userRepositoryMock.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _userService.AddUserAsync(dto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Message.Should().Be("Kullanıcı başarıyla oluşturuldu.");
        _userRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<UserEntity>()), Times.Once);
        _userRepositoryMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task AddUserAsync_ShouldReturnFailure_WhenValidationFails()
    {
        // Arrange
        var dto = new UserCreateDto { FirstName = "", Email = "bad", PasswordHash = "p" };

        var failures = new[] { new ValidationFailure("Email", "Email invalid") };
        _validatorMock.Setup(v => v.ValidateAsync(It.IsAny<UserEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(failures));

        _passwordHasherMock.Setup(ph => ph.HashPassword(It.IsAny<string>())).Returns("hash");

        // Act
        var result = await _userService.AddUserAsync(dto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain("Email invalid");
        _userRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<UserEntity>()), Times.Never);
    }

    [Fact]
    public async Task AddUserAsync_ShouldReturnFailure_WhenEmailAlreadyExists()
    {
        // Arrange
        var dto = new UserCreateDto
        {
            FirstName = "Jane",
            LastName = "Smith",
            Email = "existing@test.com",
            PasswordHash = "password"
        };

        var existingUser = new UserEntity { Email = "existing@test.com" };
        _passwordHasherMock.Setup(ph => ph.HashPassword("password")).Returns("hash");
        _validatorMock.Setup(v => v.ValidateAsync(It.IsAny<UserEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        _userRepositoryMock.Setup(repo => repo.GetAllAsync()).ReturnsAsync(new List<UserEntity> { existingUser });

        // Act
        var result = await _userService.AddUserAsync(dto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("Bu e-posta adresi ile zaten bir kayıt mevcut!");
        _userRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<UserEntity>()), Times.Never);
    }

    [Fact]
    public async Task AddUserAsync_ShouldHashPassword_WhenCreatingUser()
    {
        // Arrange
        var plainPassword = "MySecurePassword123";
        var hashedPassword = "hashed_MySecurePassword123";
        var dto = new UserCreateDto
        {
            FirstName = "Test",
            LastName = "User",
            Email = "test@example.com",
            PasswordHash = plainPassword
        };

        _passwordHasherMock.Setup(ph => ph.HashPassword(plainPassword)).Returns(hashedPassword);
        _validatorMock.Setup(v => v.ValidateAsync(It.IsAny<UserEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        _userRepositoryMock.Setup(repo => repo.GetAllAsync()).ReturnsAsync(new List<UserEntity>());

        UserEntity capturedUser = null;
        _userRepositoryMock.Setup(repo => repo.AddAsync(It.IsAny<UserEntity>()))
            .Callback<UserEntity>(u => capturedUser = u)
            .Returns(Task.CompletedTask);
        _userRepositoryMock.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _userService.AddUserAsync(dto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        capturedUser.PasswordHash.Should().Be(hashedPassword);
        capturedUser.IsActive.Should().BeTrue();
        capturedUser.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, precision: TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task AddUserAsync_ShouldReturnFailure_WithMultipleValidationErrors()
    {
        // Arrange
        var dto = new UserCreateDto { FirstName = "", Email = "", PasswordHash = "" };

        var failures = new[]
        {
            new ValidationFailure("FirstName", "First name required"),
            new ValidationFailure("Email", "Email required")
        };
        _validatorMock.Setup(v => v.ValidateAsync(It.IsAny<UserEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(failures));

        _passwordHasherMock.Setup(ph => ph.HashPassword(It.IsAny<string>())).Returns("hash");

        // Act
        var result = await _userService.AddUserAsync(dto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain("First name required");
        result.Message.Should().Contain("Email required");
    }

    #endregion

    #region UpdateUserAsync Tests

    [Fact]
    public async Task UpdateUserAsync_ShouldReturnSuccess_WhenUserExistsAndValidationPasses()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var existingUser = new UserEntity { Id = userId, FirstName = "Old", LastName = "Name" };
        var dto = new UserUpdateDto { Id = userId, FirstName = "New", LastName = "Name" };

        _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync(existingUser);
        _validatorMock.Setup(v => v.ValidateAsync(It.IsAny<UserEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        _userRepositoryMock.Setup(repo => repo.Update(It.IsAny<UserEntity>()));
        _userRepositoryMock.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _userService.UpdateUserAsync(dto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Message.Should().Be("Kullanıcı başarıyla güncellendi.");
        _userRepositoryMock.Verify(repo => repo.Update(It.IsAny<UserEntity>()), Times.Once);
        _userRepositoryMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateUserAsync_ShouldReturnFailure_WhenUserNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var dto = new UserUpdateDto { Id = userId, FirstName = "Test" };

        _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync((UserEntity)null);

        // Act
        var result = await _userService.UpdateUserAsync(dto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("Güncellenecek kullanıcı bulunamadı.");
        _userRepositoryMock.Verify(repo => repo.Update(It.IsAny<UserEntity>()), Times.Never);
    }

    [Fact]
    public async Task UpdateUserAsync_ShouldReturnFailure_WhenValidationFails()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var existingUser = new UserEntity { Id = userId, FirstName = "Old" };
        var dto = new UserUpdateDto { Id = userId, FirstName = "" };

        var failures = new[] { new ValidationFailure("FirstName", "Name required") };
        _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync(existingUser);
        _validatorMock.Setup(v => v.ValidateAsync(It.IsAny<UserEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(failures));

        // Act
        var result = await _userService.UpdateUserAsync(dto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain("Name required");
        _userRepositoryMock.Verify(repo => repo.Update(It.IsAny<UserEntity>()), Times.Never);
    }

    #endregion

    #region DeleteUserAsync Tests

    [Fact]
    public async Task DeleteUserAsync_ShouldReturnSuccess_WhenUserExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new UserEntity { Id = userId };
        _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync(user);
        _userRepositoryMock.Setup(repo => repo.Remove(It.IsAny<UserEntity>()));
        _userRepositoryMock.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _userService.DeleteUserAsync(userId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Message.Should().Be("Kullanıcı başarıyla silindi.");
        _userRepositoryMock.Verify(repo => repo.Remove(It.IsAny<UserEntity>()), Times.Once);
        _userRepositoryMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteUserAsync_ShouldReturnFailure_WhenUserNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync((UserEntity)null);

        // Act
        var result = await _userService.DeleteUserAsync(userId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("Silinecek kullanıcı bulunamadı.");
        _userRepositoryMock.Verify(repo => repo.Remove(It.IsAny<UserEntity>()), Times.Never);
    }

    #endregion

    #region AddBalanceAsync Tests

    [Fact]
    public async Task AddBalanceAsync_ShouldReturnSuccess_WhenAmountIsPositiveAndUserExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new UserEntity { Id = userId, Balance = 100m };
        var amount = 50m;

        _workContextMock.SetupGet(wc => wc.UserId).Returns(userId);
        _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync(user);
        _userRepositoryMock.Setup(repo => repo.Update(It.IsAny<UserEntity>()));
        _userRepositoryMock.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _userService.AddBalanceAsync(amount);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Message.Should().Be("Bakiye başarıyla yüklendi.");
        user.Balance.Should().Be(150m); // 100 + 50
        _userRepositoryMock.Verify(repo => repo.Update(It.IsAny<UserEntity>()), Times.Once);
    }

    [Fact]
    public async Task AddBalanceAsync_ShouldReturnFailure_WhenAmountIsZero()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _workContextMock.SetupGet(wc => wc.UserId).Returns(userId);

        // Act
        var result = await _userService.AddBalanceAsync(0m);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("Yüklenecek tutar 0'dan büyük olmalıdır.");
        _userRepositoryMock.Verify(repo => repo.Update(It.IsAny<UserEntity>()), Times.Never);
    }

    [Fact]
    public async Task AddBalanceAsync_ShouldReturnFailure_WhenAmountIsNegative()
    {
        // Arrange
        _workContextMock.SetupGet(wc => wc.UserId).Returns(Guid.NewGuid());

        // Act
        var result = await _userService.AddBalanceAsync(-25m);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("Yüklenecek tutar 0'dan büyük olmalıdır.");
    }

    [Fact]
    public async Task AddBalanceAsync_ShouldReturnFailure_WhenUserNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _workContextMock.SetupGet(wc => wc.UserId).Returns(userId);
        _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync((UserEntity)null);

        // Act
        var result = await _userService.AddBalanceAsync(50m);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("Kullanıcı bulunamadı.");
        _userRepositoryMock.Verify(repo => repo.Update(It.IsAny<UserEntity>()), Times.Never);
    }

    #endregion

    #region ChangePasswordAsync Tests

    [Fact]
    public async Task ChangePasswordAsync_ShouldReturnSuccess_WhenCurrentPasswordCorrectAndNewPasswordsMatch()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var currentPassword = "oldpassword";
        var newPassword = "newpassword";
        var oldHash = BCrypt.Net.BCrypt.HashPassword(currentPassword);
        var newHash = "newhashedpassword";

        var user = new UserEntity { Id = userId, PasswordHash = oldHash };
        var request = new ChangePasswordDto
        {
            CurrentPassword = currentPassword,
            NewPassword = newPassword,
            ConfirmPassword = newPassword
        };

        _workContextMock.SetupGet(wc => wc.UserId).Returns(userId);
        _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync(user);
        _passwordHasherMock.Setup(ph => ph.VerifyPassword(currentPassword, oldHash)).Returns(true);
        _passwordHasherMock.Setup(ph => ph.HashPassword(newPassword)).Returns(newHash);
        _userRepositoryMock.Setup(repo => repo.Update(It.IsAny<UserEntity>()));
        _userRepositoryMock.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _userService.ChangePasswordAsync(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Message.Should().Be("Şifreniz başarıyla güncellendi.");
        user.PasswordHash.Should().Be(newHash);
        _userRepositoryMock.Verify(repo => repo.Update(It.IsAny<UserEntity>()), Times.Once);
    }

    [Fact]
    public async Task ChangePasswordAsync_ShouldReturnFailure_WhenNewPasswordsDoNotMatch()
    {
        // Arrange
        var request = new ChangePasswordDto
        {
            CurrentPassword = "old",
            NewPassword = "new1",
            ConfirmPassword = "new2"
        };

        _workContextMock.SetupGet(wc => wc.UserId).Returns(Guid.NewGuid());

        // Act
        var result = await _userService.ChangePasswordAsync(request);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("Yeni şifreler birbiriyle eşleşmiyor.");
        _userRepositoryMock.Verify(repo => repo.Update(It.IsAny<UserEntity>()), Times.Never);
    }

    [Fact]
    public async Task ChangePasswordAsync_ShouldReturnFailure_WhenUserNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new ChangePasswordDto
        {
            CurrentPassword = "old",
            NewPassword = "new",
            ConfirmPassword = "new"
        };

        _workContextMock.SetupGet(wc => wc.UserId).Returns(userId);
        _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync((UserEntity)null);

        // Act
        var result = await _userService.ChangePasswordAsync(request);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("Kullanıcı bulunamadı.");
        _userRepositoryMock.Verify(repo => repo.Update(It.IsAny<UserEntity>()), Times.Never);
    }

    [Fact]
    public async Task ChangePasswordAsync_ShouldReturnFailure_WhenCurrentPasswordIsWrong()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new UserEntity { Id = userId, PasswordHash = "somehash" };
        var request = new ChangePasswordDto
        {
            CurrentPassword = "wrongpassword",
            NewPassword = "newpassword",
            ConfirmPassword = "newpassword"
        };

        _workContextMock.SetupGet(wc => wc.UserId).Returns(userId);
        _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync(user);
        _passwordHasherMock.Setup(ph => ph.VerifyPassword("wrongpassword", "somehash")).Returns(false);

        // Act
        var result = await _userService.ChangePasswordAsync(request);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("Mevcut şifreniz yanlış.");
        _userRepositoryMock.Verify(repo => repo.Update(It.IsAny<UserEntity>()), Times.Never);
    }

    #endregion

    #region UpdateProfileAsync Tests

    [Fact]
    public async Task UpdateProfileAsync_ShouldReturnSuccess_WhenUserExistsAndProfileUpdates()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new UserEntity { Id = userId, FirstName = "Old", LastName = "Name", Location = "Istanbul" };
        var newToken = "newtoken123";
        var request = new UpdateProfileDto
        {
            FirstName = "New",
            LastName = "Updated",
            Location = "Ankara"
        };

        _workContextMock.SetupGet(wc => wc.UserId).Returns(userId);
        _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync(user);
        _userRepositoryMock.Setup(repo => repo.Update(It.IsAny<UserEntity>()));
        _userRepositoryMock.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(1);
        _jwtProviderMock.Setup(jp => jp.GenerateToken(It.IsAny<UserEntity>())).Returns(newToken);

        // Act
        var result = await _userService.UpdateProfileAsync(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().Be(newToken);
        result.Message.Should().Be("Profil bilgileri başarıyla güncellendi.");
        user.FirstName.Should().Be("New");
        user.LastName.Should().Be("Updated");
        user.Location.Should().Be("Ankara");
        _userRepositoryMock.Verify(repo => repo.Update(It.IsAny<UserEntity>()), Times.Once);
        _jwtProviderMock.Verify(jp => jp.GenerateToken(It.IsAny<UserEntity>()), Times.Once);
    }

    [Fact]
    public async Task UpdateProfileAsync_ShouldReturnFailure_WhenUserNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new UpdateProfileDto { FirstName = "Test", LastName = "User" };

        _workContextMock.SetupGet(wc => wc.UserId).Returns(userId);
        _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync((UserEntity)null);

        // Act
        var result = await _userService.UpdateProfileAsync(request);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("Kullanıcı bulunamadı.");
        _userRepositoryMock.Verify(repo => repo.Update(It.IsAny<UserEntity>()), Times.Never);
    }

    [Fact]
    public async Task UpdateProfileAsync_ShouldUpdateAllProfileFields_WhenProvided()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new UserEntity { Id = userId, FirstName = "", LastName = "", Location = "" };
        var newToken = "token";
        var request = new UpdateProfileDto
        {
            FirstName = "John",
            LastName = "Doe",
            Location = "Istanbul"
        };

        _workContextMock.SetupGet(wc => wc.UserId).Returns(userId);
        _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync(user);
        _userRepositoryMock.Setup(repo => repo.Update(It.IsAny<UserEntity>()));
        _userRepositoryMock.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(1);
        _jwtProviderMock.Setup(jp => jp.GenerateToken(user)).Returns(newToken);

        // Act
        var result = await _userService.UpdateProfileAsync(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        user.FirstName.Should().Be("John");
        user.LastName.Should().Be("Doe");
        user.Location.Should().Be("Istanbul");
    }

    #endregion
}
