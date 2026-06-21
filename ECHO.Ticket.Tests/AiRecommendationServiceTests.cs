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

public class AiRecommendationServiceTests
{
    [Fact]
    public async Task GetEventScorePredictionAsync_ShouldReturnScore_WhenApiReturnsSuccess()
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{\"predicted_score\": 4.5}")
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

        var service = new AiRecommendationService(httpClient, mockConfig.Object);

        // Act
        var result = await service.GetEventScorePredictionAsync("user123", "Music", "Istanbul");

        // Assert
        result.Should().Be(4.5);
    }

    [Fact]
    public async Task GetEventScorePredictionAsync_ShouldReturnNeutral_WhenApiFails()
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        var responseMessage = new HttpResponseMessage(HttpStatusCode.InternalServerError);

        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(responseMessage);

        var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        var mockConfig = new Mock<IConfiguration>();

        var service = new AiRecommendationService(httpClient, mockConfig.Object);

        // Act
        var result = await service.GetEventScorePredictionAsync("user123", "Sports", "Ankara");

        // Assert
        result.Should().Be(3.0);
    }

    [Fact]
    public async Task GetEventScorePredictionAsync_ShouldReturnNeutral_WhenExceptionThrown()
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Connection failed"));

        var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        var mockConfig = new Mock<IConfiguration>();

        var service = new AiRecommendationService(httpClient, mockConfig.Object);

        // Act
        var result = await service.GetEventScorePredictionAsync("user456", "Theater", "Izmir");

        // Assert
        result.Should().Be(3.0);
    }

    [Fact]
    public async Task GetEventScorePredictionAsync_ShouldReturnCorrectScore_WithVariousValues()
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{\"predicted_score\": 2.1}")
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

        var service = new AiRecommendationService(httpClient, mockConfig.Object);

        // Act
        var result = await service.GetEventScorePredictionAsync("user789", "Art", "Bursa");

        // Assert
        result.Should().Be(2.1);
    }
}