using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ECHO.Ticket.Business.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using Xunit;

namespace ECHO.Ticket.Tests;

public class SentimentAnalysisServiceTests
{
    [Fact]
    public async Task AnalyzeReviewAsync_ShouldReturnPositive_WhenApiReturnsPositiveSentiment()
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{\"sentiment\": \"POSITIVE\", \"confidence_score\": 95.5}")
        };

        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(responseMessage);

        var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        var mockConfig = new Mock<IConfiguration>(); // Test için sahte ayar

        var service = new SentimentAnalysisService(httpClient, mockConfig.Object);

        // Act
        var (label, score) = await service.AnalyzeReviewAsync("This event was amazing!");

        // Assert
        label.Should().Be("POSITIVE");
        score.Should().Be(95.5);
    }

    [Fact]
    public async Task AnalyzeReviewAsync_ShouldReturnNegative_WhenApiReturnsNegativeSentiment()
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{\"sentiment\": \"NEGATIVE\", \"confidence_score\": 87.3}")
        };

        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(responseMessage);

        var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        var mockConfig = new Mock<IConfiguration>();

        var service = new SentimentAnalysisService(httpClient, mockConfig.Object);

        // Act
        var (label, score) = await service.AnalyzeReviewAsync("Terrible experience");

        // Assert
        label.Should().Be("NEGATIVE");
        score.Should().Be(87.3);
    }

    [Fact]
    public async Task AnalyzeReviewAsync_ShouldReturnNeutral_WhenApiFails()
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        var responseMessage = new HttpResponseMessage(HttpStatusCode.ServiceUnavailable);

        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(responseMessage);

        var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        var mockConfig = new Mock<IConfiguration>();

        var service = new SentimentAnalysisService(httpClient, mockConfig.Object);

        // Act
        var (label, score) = await service.AnalyzeReviewAsync("Some review");

        // Assert
        label.Should().Be("NEUTRAL");
        score.Should().Be(0);
    }

    [Fact]
    public async Task AnalyzeReviewAsync_ShouldReturnNeutral_WhenExceptionThrown()
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Server unreachable"));

        var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        var mockConfig = new Mock<IConfiguration>();

        var service = new SentimentAnalysisService(httpClient, mockConfig.Object);

        // Act
        var (label, score) = await service.AnalyzeReviewAsync("Review text");

        // Assert
        label.Should().Be("NEUTRAL");
        score.Should().Be(0);
    }

    [Fact]
    public async Task AnalyzeReviewAsync_ShouldReturnNeutral_WhenApiReturnsNeutralSentiment()
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{\"sentiment\": \"NEUTRAL\", \"confidence_score\": 50.0}")
        };

        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(responseMessage);

        var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        var mockConfig = new Mock<IConfiguration>();

        var service = new SentimentAnalysisService(httpClient, mockConfig.Object);

        // Act
        var (label, score) = await service.AnalyzeReviewAsync("It was okay");

        // Assert
        label.Should().Be("NEUTRAL");
        score.Should().Be(50.0);
    }
}