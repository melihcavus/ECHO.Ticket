using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using ECHO.Ticket.Business.Services;
using ECHO.Ticket.Business.Interfaces;
using ECHO.Ticket.Core.DTOs;
using ECHO.Ticket.Core.Entities;
using ECHO.Ticket.Core.Results;
using ECHO.Ticket.DataAccess.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace ECHO.Ticket.Tests;

public class EventReviewServiceTests
{
    private readonly Mock<IRepository<EventReview>> _reviewRepositoryMock;
    private readonly Mock<IRepository<User>> _userRepositoryMock;
    private readonly Mock<IWorkContext> _workContextMock;
    private readonly Mock<ISentimentAnalysisService> _sentimentServiceMock;
    private readonly EventReviewService _eventReviewService;

    public EventReviewServiceTests()
    {
        _reviewRepositoryMock = new Mock<IRepository<EventReview>>();
        _userRepositoryMock = new Mock<IRepository<User>>();
        _workContextMock = new Mock<IWorkContext>();
        _sentimentServiceMock = new Mock<ISentimentAnalysisService>();

        _eventReviewService = new EventReviewService(
            _reviewRepositoryMock.Object,
            _userRepositoryMock.Object,
            _workContextMock.Object,
            _sentimentServiceMock.Object);
    }

    #region AddReviewAsync Tests

    [Fact]
    public async Task AddReviewAsync_ShouldReturnSuccess_WhenReviewIsValid()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        var reviewDto = new EventReviewCreateDto
        {
            EventId = eventId,
            Rating = 4,
            Content = "Great event!"
        };

        _workContextMock.SetupGet(wc => wc.UserId).Returns(userId);
        _sentimentServiceMock.Setup(s => s.AnalyzeReviewAsync("Great event!"))
            .ReturnsAsync(("POSITIVE", 85.5));

        EventReview capturedReview = null;
        _reviewRepositoryMock.Setup(r => r.AddAsync(It.IsAny<EventReview>()))
            .Callback<EventReview>(ev => capturedReview = ev)
            .Returns(Task.CompletedTask);
        _reviewRepositoryMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _eventReviewService.AddReviewAsync(reviewDto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Message.Should().Be("Yorumunuz başarıyla kaydedildi ve yapay zeka tarafından analiz edildi.");
        capturedReview.Should().NotBeNull();
        capturedReview.UserId.Should().Be(userId);
        capturedReview.EventId.Should().Be(eventId);
        capturedReview.Rating.Should().Be(4);
        capturedReview.SentimentLabel.Should().Be("POSITIVE");
        capturedReview.SentimentScore.Should().Be(85.5);
        capturedReview.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, precision: TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task AddReviewAsync_ShouldReturnFailure_WhenRatingBelow1()
    {
        // Arrange
        var reviewDto = new EventReviewCreateDto
        {
            EventId = Guid.NewGuid(),
            Rating = 0,
            Content = "Bad"
        };

        _workContextMock.SetupGet(wc => wc.UserId).Returns(Guid.NewGuid());

        // Act
        var result = await _eventReviewService.AddReviewAsync(reviewDto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("Hata: Puanlama 1 ile 5 yıldız arasında olmalıdır.");
        _reviewRepositoryMock.Verify(r => r.AddAsync(It.IsAny<EventReview>()), Times.Never);
    }

    [Fact]
    public async Task AddReviewAsync_ShouldReturnFailure_WhenRatingAbove5()
    {
        // Arrange
        var reviewDto = new EventReviewCreateDto
        {
            EventId = Guid.NewGuid(),
            Rating = 6,
            Content = "Excellent"
        };

        _workContextMock.SetupGet(wc => wc.UserId).Returns(Guid.NewGuid());

        // Act
        var result = await _eventReviewService.AddReviewAsync(reviewDto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("Hata: Puanlama 1 ile 5 yıldız arasında olmalıdır.");
        _reviewRepositoryMock.Verify(r => r.AddAsync(It.IsAny<EventReview>()), Times.Never);
    }

    [Fact]
    public async Task AddReviewAsync_ShouldReturnFailure_WhenRatingIs10()
    {
        // Arrange
        var reviewDto = new EventReviewCreateDto
        {
            EventId = Guid.NewGuid(),
            Rating = 10,
            Content = "Perfect"
        };

        _workContextMock.SetupGet(wc => wc.UserId).Returns(Guid.NewGuid());

        // Act
        var result = await _eventReviewService.AddReviewAsync(reviewDto);

        // Assert
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task AddReviewAsync_ShouldCallSentimentService_WithReviewContent()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var reviewContent = "Amazing experience!";
        var reviewDto = new EventReviewCreateDto
        {
            EventId = Guid.NewGuid(),
            Rating = 5,
            Content = reviewContent
        };

        _workContextMock.SetupGet(wc => wc.UserId).Returns(userId);
        _sentimentServiceMock.Setup(s => s.AnalyzeReviewAsync(reviewContent))
            .ReturnsAsync(("POSITIVE", 95.0));

        _reviewRepositoryMock.Setup(r => r.AddAsync(It.IsAny<EventReview>())).Returns(Task.CompletedTask);
        _reviewRepositoryMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        await _eventReviewService.AddReviewAsync(reviewDto);

        // Assert
        _sentimentServiceMock.Verify(s => s.AnalyzeReviewAsync(reviewContent), Times.Once);
    }

    [Fact]
    public async Task AddReviewAsync_ShouldUseNegativeSentiment_WhenDetected()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var reviewDto = new EventReviewCreateDto
        {
            EventId = Guid.NewGuid(),
            Rating = 1,
            Content = "Terrible event"
        };

        _workContextMock.SetupGet(wc => wc.UserId).Returns(userId);
        _sentimentServiceMock.Setup(s => s.AnalyzeReviewAsync("Terrible event"))
            .ReturnsAsync(("NEGATIVE", 92.3));

        EventReview capturedReview = null;
        _reviewRepositoryMock.Setup(r => r.AddAsync(It.IsAny<EventReview>()))
            .Callback<EventReview>(ev => capturedReview = ev)
            .Returns(Task.CompletedTask);
        _reviewRepositoryMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _eventReviewService.AddReviewAsync(reviewDto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        capturedReview.SentimentLabel.Should().Be("NEGATIVE");
        capturedReview.SentimentScore.Should().Be(92.3);
    }

    #endregion

    #region GetReviewsByEventIdAsync Tests

    [Fact]
    public async Task GetReviewsByEventIdAsync_ShouldReturnReviews_WithUserNames()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var user1Id = Guid.NewGuid();
        var user2Id = Guid.NewGuid();

        var reviews = new List<EventReview>
        {
            new EventReview
            {
                Id = Guid.NewGuid(),
                EventId = eventId,
                UserId = user1Id,
                Rating = 5,
                Content = "Excellent",
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                SentimentLabel = "POSITIVE",
                SentimentScore = 95
            },
            new EventReview
            {
                Id = Guid.NewGuid(),
                EventId = eventId,
                UserId = user2Id,
                Rating = 3,
                Content = "Good",
                CreatedAt = DateTime.UtcNow,
                SentimentLabel = "NEUTRAL",
                SentimentScore = 50
            }
        };

        var users = new List<User>
        {
            new User { Id = user1Id, FirstName = "John", LastName = "Doe" },
            new User { Id = user2Id, FirstName = "Jane", LastName = "Smith" }
        };

        _reviewRepositoryMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<EventReview, bool>>>()))
            .ReturnsAsync(reviews);
        _userRepositoryMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>()))
            .ReturnsAsync(users);

        // Act
        var result = await _eventReviewService.GetReviewsByEventIdAsync(eventId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var dtos = result.Data!.ToList();
        dtos.Should().HaveCount(2);
        dtos[0].UserFullName.Should().Be("Jane Smith"); // Most recent first
        dtos[1].UserFullName.Should().Be("John Doe");
    }

    [Fact]
    public async Task GetReviewsByEventIdAsync_ShouldShowDefaultName_WhenUserNotFound()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var reviews = new List<EventReview>
        {
            new EventReview
            {
                Id = Guid.NewGuid(),
                EventId = eventId,
                UserId = userId,
                Rating = 4,
                Content = "Nice",
                CreatedAt = DateTime.UtcNow,
                SentimentLabel = "POSITIVE",
                SentimentScore = 80
            }
        };

        _reviewRepositoryMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<EventReview, bool>>>()))
            .ReturnsAsync(reviews);
        _userRepositoryMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>()))
            .ReturnsAsync(new List<User>());

        // Act
        var result = await _eventReviewService.GetReviewsByEventIdAsync(eventId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var dtos = result.Data!.ToList();
        dtos.Should().HaveCount(1);
        dtos[0].UserFullName.Should().Be("ECHO Kullanıcısı");
    }

    [Fact]
    public async Task GetReviewsByEventIdAsync_ShouldReturnEmpty_WhenNoReviewsExist()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        _reviewRepositoryMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<EventReview, bool>>>()))
            .ReturnsAsync(new List<EventReview>());

        // Act
        var result = await _eventReviewService.GetReviewsByEventIdAsync(eventId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeEmpty();
    }

    [Fact]
    public async Task GetReviewsByEventIdAsync_ShouldOrderByCreatedAtDescending()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var now = DateTime.UtcNow;

        var reviews = new List<EventReview>
        {
            new EventReview { Id = Guid.NewGuid(), EventId = eventId, UserId = userId, Rating = 1, Content = "Old", CreatedAt = now.AddDays(-3) },
            new EventReview { Id = Guid.NewGuid(), EventId = eventId, UserId = userId, Rating = 5, Content = "New", CreatedAt = now },
            new EventReview { Id = Guid.NewGuid(), EventId = eventId, UserId = userId, Rating = 3, Content = "Mid", CreatedAt = now.AddDays(-1) }
        };

        _reviewRepositoryMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<EventReview, bool>>>()))
            .ReturnsAsync(reviews);
        _userRepositoryMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>()))
            .ReturnsAsync(new List<User> { new User { Id = userId, FirstName = "A", LastName = "B" } });

        // Act
        var result = await _eventReviewService.GetReviewsByEventIdAsync(eventId);

        // Assert
        var dtos = result.Data!.ToList();
        dtos[0].Content.Should().Be("New");
        dtos[1].Content.Should().Be("Mid");
        dtos[2].Content.Should().Be("Old");
    }

    #endregion

    #region GetEventAnalyticsAsync Tests

    [Fact]
    public async Task GetEventAnalyticsAsync_ShouldReturnZeroSatisfaction_WhenNoReviews()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        _reviewRepositoryMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<EventReview, bool>>>()))
            .ReturnsAsync(new List<EventReview>());

        // Act
        var result = await _eventReviewService.GetEventAnalyticsAsync(eventId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var analytics = result.Data!;
        analytics.EventId.Should().Be(eventId);
        analytics.SatisfactionScore.Should().Be(0);
        analytics.TotalReviews.Should().Be(0);
        result.Message.Should().Be("Bu etkinlik için henüz yorum yapılmamış.");
    }

    [Fact]
    public async Task GetEventAnalyticsAsync_ShouldCalculateSentimentCounts_Correctly()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var reviews = new List<EventReview>
        {
            new EventReview { Id = Guid.NewGuid(), EventId = eventId, UserId = userId, Rating = 5, SentimentLabel = "POSITIVE", SentimentScore = 95 },
            new EventReview { Id = Guid.NewGuid(), EventId = eventId, UserId = userId, Rating = 5, SentimentLabel = "POSITIVE", SentimentScore = 85 },
            new EventReview { Id = Guid.NewGuid(), EventId = eventId, UserId = userId, Rating = 1, SentimentLabel = "NEGATIVE", SentimentScore = 90 },
            new EventReview { Id = Guid.NewGuid(), EventId = eventId, UserId = userId, Rating = 3, SentimentLabel = "NEUTRAL", SentimentScore = 50 }
        };

        _reviewRepositoryMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<EventReview, bool>>>()))
            .ReturnsAsync(reviews);
        _userRepositoryMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>()))
            .ReturnsAsync(new List<User>());

        // Act
        var result = await _eventReviewService.GetEventAnalyticsAsync(eventId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var analytics = result.Data!;
        analytics.TotalReviews.Should().Be(4);
        analytics.PositiveCount.Should().Be(2);
        analytics.NegativeCount.Should().Be(1);
        analytics.NeutralCount.Should().Be(1);
    }

    [Fact]
    public async Task GetEventAnalyticsAsync_ShouldCalculateSatisfactionScore_AsPercentage()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var reviews = new List<EventReview>
        {
            new EventReview { Id = Guid.NewGuid(), EventId = eventId, UserId = userId, SentimentLabel = "POSITIVE", SentimentScore = 85 },
            new EventReview { Id = Guid.NewGuid(), EventId = eventId, UserId = userId, SentimentLabel = "POSITIVE", SentimentScore = 90 },
            new EventReview { Id = Guid.NewGuid(), EventId = eventId, UserId = userId, SentimentLabel = "NEUTRAL", SentimentScore = 50 },
            new EventReview { Id = Guid.NewGuid(), EventId = eventId, UserId = userId, SentimentLabel = "NEGATIVE", SentimentScore = 85 }
        };

        _reviewRepositoryMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<EventReview, bool>>>()))
            .ReturnsAsync(reviews);
        _userRepositoryMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>()))
            .ReturnsAsync(new List<User>());

        // Act
        var result = await _eventReviewService.GetEventAnalyticsAsync(eventId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var analytics = result.Data!;
        // 2 positive out of 4 total = 50%
        analytics.SatisfactionScore.Should().Be(50.0);
    }

    [Fact]
    public async Task GetEventAnalyticsAsync_ShouldFilterByThreshold80_ForPositive()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var reviews = new List<EventReview>
        {
            new EventReview { Id = Guid.NewGuid(), EventId = eventId, UserId = userId, SentimentLabel = "POSITIVE", SentimentScore = 85 }, // >= 80: counted
            new EventReview { Id = Guid.NewGuid(), EventId = eventId, UserId = userId, SentimentLabel = "POSITIVE", SentimentScore = 75 }  // < 80: NOT counted
        };

        _reviewRepositoryMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<EventReview, bool>>>()))
            .ReturnsAsync(reviews);
        _userRepositoryMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>()))
            .ReturnsAsync(new List<User>());

        // Act
        var result = await _eventReviewService.GetEventAnalyticsAsync(eventId);

        // Assert
        var analytics = result.Data!;
        analytics.PositiveCount.Should().Be(1); // Only score 85 is counted
        analytics.NeutralCount.Should().Be(1);  // Score 75 falls into neutral
    }

    [Fact]
    public async Task GetEventAnalyticsAsync_ShouldReturnTop5CriticalReviews_WhenMoreThan5NegativeExist()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var reviews = new List<EventReview>();
        for (int i = 0; i < 8; i++)
        {
            reviews.Add(new EventReview
            {
                Id = Guid.NewGuid(),
                EventId = eventId,
                UserId = userId,
                Content = $"Negative {i}",
                SentimentLabel = "NEGATIVE",
                SentimentScore = 90 - i  // 90, 89, 88, 87, 86, 85, 84, 83
            });
        }

        _reviewRepositoryMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<EventReview, bool>>>()))
            .ReturnsAsync(reviews);
        _userRepositoryMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>()))
            .ReturnsAsync(new List<User> { new User { Id = userId, FirstName = "Test", LastName = "User" } });

        // Act
        var result = await _eventReviewService.GetEventAnalyticsAsync(eventId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var analytics = result.Data!;
        analytics.CriticalReviews.Should().HaveCount(5);
        // Should be ordered by score descending
        analytics.CriticalReviews[0].SentimentScore.Should().Be(90);
        analytics.CriticalReviews[4].SentimentScore.Should().Be(86);
    }

    [Fact]
    public async Task GetEventAnalyticsAsync_ShouldReturnEmpty_WhenNoCriticalReviews()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var reviews = new List<EventReview>
        {
            new EventReview { Id = Guid.NewGuid(), EventId = eventId, UserId = userId, SentimentLabel = "POSITIVE", SentimentScore = 95 },
            new EventReview { Id = Guid.NewGuid(), EventId = eventId, UserId = userId, SentimentLabel = "NEUTRAL", SentimentScore = 50 }
        };

        _reviewRepositoryMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<EventReview, bool>>>()))
            .ReturnsAsync(reviews);
        _userRepositoryMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>()))
            .ReturnsAsync(new List<User>());

        // Act
        var result = await _eventReviewService.GetEventAnalyticsAsync(eventId);

        // Assert
        var analytics = result.Data!;
        analytics.CriticalReviews.Should().BeEmpty();
    }

    [Fact]
    public async Task GetEventAnalyticsAsync_ShouldRoundSentimentScore_ToTwoDecimals()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var reviews = new List<EventReview>
        {
            new EventReview
            {
                Id = Guid.NewGuid(),
                EventId = eventId,
                UserId = userId,
                SentimentLabel = "NEGATIVE",
                SentimentScore = 92.3456
            }
        };

        _reviewRepositoryMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<EventReview, bool>>>()))
            .ReturnsAsync(reviews);
        _userRepositoryMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>()))
            .ReturnsAsync(new List<User> { new User { Id = userId, FirstName = "A", LastName = "B" } });

        // Act
        var result = await _eventReviewService.GetEventAnalyticsAsync(eventId);

        // Assert
        var analytics = result.Data!;
        analytics.CriticalReviews[0].SentimentScore.Should().Be(92.35);
    }

    [Fact]
    public async Task GetEventAnalyticsAsync_ShouldHandleNullSentimentScore_AsZero()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var reviews = new List<EventReview>
        {
            new EventReview
            {
                Id = Guid.NewGuid(),
                EventId = eventId,
                UserId = userId,
                SentimentLabel = "NEGATIVE",
                SentimentScore = null  // Null score
            }
        };

        _reviewRepositoryMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<EventReview, bool>>>()))
            .ReturnsAsync(reviews);
        _userRepositoryMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>()))
            .ReturnsAsync(new List<User> { new User { Id = userId, FirstName = "X", LastName = "Y" } });

        // Act
        var result = await _eventReviewService.GetEventAnalyticsAsync(eventId);

        // Assert
        var analytics = result.Data!;
        analytics.CriticalReviews[0].SentimentScore.Should().Be(0);
    }

    #endregion
}
