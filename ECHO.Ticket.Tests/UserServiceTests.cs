using ECHO.Ticket.Business.Services;
using ECHO.Ticket.Core.DTOs;
using ECHO.Ticket.Core.Entities;
using ECHO.Ticket.Core.Results;
using ECHO.Ticket.DataAccess.Interfaces;
using ECHO.Ticket.Business.Interfaces;
using FluentAssertions;
using FluentValidation;
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
    private readonly UserService _userService;

    public UserServiceTests()
    {
        _userRepositoryMock = new Mock<IRepository<UserEntity>>();
        _validatorMock = new Mock<IValidator<UserEntity>>();
        _jwtProviderMock = new Mock<IJwtProvider>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _userService = new UserService(_userRepositoryMock.Object, _validatorMock.Object, _jwtProviderMock.Object, _passwordHasherMock.Object);
    }

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
}
