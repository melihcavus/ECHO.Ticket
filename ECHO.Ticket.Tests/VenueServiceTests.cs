using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using ECHO.Ticket.Business.Services;
using ECHO.Ticket.Core.DTOs;
using ECHO.Ticket.Core.Entities;
using ECHO.Ticket.Core.Results;
using ECHO.Ticket.DataAccess.Contexts;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Moq;
using Xunit;

namespace ECHO.Ticket.Tests;

public class VenueServiceTests
{
    private readonly Mock<EchoDbContext> _contextMock;
    private readonly Mock<IDistributedCache> _cacheMock;
    private readonly VenueService _venueService;

    public VenueServiceTests()
    {
        _contextMock = new Mock<EchoDbContext>();
        _cacheMock = new Mock<IDistributedCache>();

        _venueService = new VenueService(
            _contextMock.Object,
            _cacheMock.Object);
    }

    #region GetAllVenuesAsync Tests

    [Fact]
    public async Task GetAllVenuesAsync_ShouldReturnCachedVenues_WhenCacheHit()
    {
        // Arrange
        var venues = new List<VenueDto>
        {
            new VenueDto { Id = Guid.NewGuid(), Name = "Venue1", Rows = 10, Columns = 20 },
            new VenueDto { Id = Guid.NewGuid(), Name = "Venue2", Rows = 15, Columns = 25 }
        };
        var serializedVenues = JsonSerializer.Serialize(venues);

        _cacheMock.Setup(c => c.GetStringAsync("all_venues", It.IsAny<CancellationToken>()))
            .ReturnsAsync(serializedVenues);

        // Act
        var result = await _venueService.GetAllVenuesAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().HaveCount(2);
        result.Data!.First().Name.Should().Be("Venue1");
        result.Data!.Last().Name.Should().Be("Venue2");
        _cacheMock.Verify(c => c.GetStringAsync("all_venues", It.IsAny<CancellationToken>()), Times.Once);
        // Database should NOT be accessed when cache hit occurs
        _contextMock.Verify(c => c.Venues, Times.Never);
    }

    [Fact]
    public async Task GetAllVenuesAsync_ShouldReturnFromDatabase_WhenCacheMiss()
    {
        // Arrange
        var venueEntities = new List<Venue>
        {
            new Venue { Id = Guid.NewGuid(), Name = "Hall A", Rows = 5, Columns = 10 },
            new Venue { Id = Guid.NewGuid(), Name = "Hall B", Rows = 8, Columns = 12 }
        };

        var mockVenuesDbSet = venueEntities.AsQueryable().BuildMockDbSet();

        _cacheMock.Setup(c => c.GetStringAsync("all_venues", It.IsAny<CancellationToken>()))
            .ReturnsAsync((string)null); // Cache miss

        _contextMock.Setup(c => c.Venues).Returns(mockVenuesDbSet.Object);

        _cacheMock.Setup(c => c.SetStringAsync("all_venues", It.IsAny<string>(), It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _venueService.GetAllVenuesAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().HaveCount(2);
        result.Data!.First().Name.Should().Be("Hall A");
        _cacheMock.Verify(c => c.GetStringAsync("all_venues", It.IsAny<CancellationToken>()), Times.Once);
        _cacheMock.Verify(c => c.SetStringAsync("all_venues", It.IsAny<string>(), It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAllVenuesAsync_ShouldReturnEmptyList_WhenNoVenuesInDatabase()
    {
        // Arrange
        var emptyVenues = new List<Venue>().AsQueryable().BuildMockDbSet();

        _cacheMock.Setup(c => c.GetStringAsync("all_venues", It.IsAny<CancellationToken>()))
            .ReturnsAsync((string)null);

        _contextMock.Setup(c => c.Venues).Returns(emptyVenues.Object);

        _cacheMock.Setup(c => c.SetStringAsync("all_venues", It.IsAny<string>(), It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _venueService.GetAllVenuesAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllVenuesAsync_ShouldCacheResults_With24HourExpiration()
    {
        // Arrange
        var venues = new List<Venue> { new Venue { Id = Guid.NewGuid(), Name = "V1", Rows = 3, Columns = 3 } };
        var mockDbSet = venues.AsQueryable().BuildMockDbSet();

        _cacheMock.Setup(c => c.GetStringAsync("all_venues", It.IsAny<CancellationToken>()))
            .ReturnsAsync((string)null);

        _contextMock.Setup(c => c.Venues).Returns(mockDbSet.Object);

        DistributedCacheEntryOptions capturedOptions = null;
        _cacheMock.Setup(c => c.SetStringAsync("all_venues", It.IsAny<string>(), It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()))
            .Callback<string, string, DistributedCacheEntryOptions, CancellationToken>((k, v, opt, ct) => capturedOptions = opt)
            .Returns(Task.CompletedTask);

        // Act
        var result = await _venueService.GetAllVenuesAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        capturedOptions.Should().NotBeNull();
        capturedOptions.AbsoluteExpirationRelativeToNow.Should().Be(TimeSpan.FromHours(24));
    }

    #endregion

    #region CreateVenueAsync Tests

    [Fact]
    public async Task CreateVenueAsync_ShouldReturnSuccess_WhenVenueCreated()
    {
        // Arrange
        var dto = new CreateVenueDto { Name = "New Venue", Rows = 10, Columns = 15 };

        var mockVenuesDbSet = new Mock<DbSet<Venue>>();
        _contextMock.Setup(c => c.Venues).Returns(mockVenuesDbSet.Object);
        _contextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _cacheMock.Setup(c => c.RemoveAsync("all_venues", It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _venueService.CreateVenueAsync(dto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Message.Should().Be("Sahne başarıyla oluşturuldu.");
        mockVenuesDbSet.Verify(m => m.AddAsync(It.IsAny<Venue>(), It.IsAny<CancellationToken>()), Times.Once);
        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _cacheMock.Verify(c => c.RemoveAsync("all_venues", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateVenueAsync_ShouldInvalidateCache_WhenVenueIsCreated()
    {
        // Arrange
        var dto = new CreateVenueDto { Name = "Test Hall", Rows = 8, Columns = 10 };

        var mockVenuesDbSet = new Mock<DbSet<Venue>>();
        _contextMock.Setup(c => c.Venues).Returns(mockVenuesDbSet.Object);
        _contextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var cacheRemoved = false;
        _cacheMock.Setup(c => c.RemoveAsync("all_venues", It.IsAny<CancellationToken>()))
            .Callback(() => cacheRemoved = true)
            .Returns(Task.CompletedTask);

        // Act
        await _venueService.CreateVenueAsync(dto);

        // Assert
        cacheRemoved.Should().BeTrue();
        _cacheMock.Verify(c => c.RemoveAsync("all_venues", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateVenueAsync_ShouldMapDtoToEntity_Correctly()
    {
        // Arrange
        var dto = new CreateVenueDto { Name = "Premier Hall", Rows = 20, Columns = 30 };

        Venue capturedVenue = null;
        var mockVenuesDbSet = new Mock<DbSet<Venue>>();
        mockVenuesDbSet.Setup(m => m.AddAsync(It.IsAny<Venue>(), It.IsAny<CancellationToken>()))
            .Callback<Venue, CancellationToken>((v, ct) => capturedVenue = v)
            .Returns(new ValueTask<Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<Venue>>());

        _contextMock.Setup(c => c.Venues).Returns(mockVenuesDbSet.Object);
        _contextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _cacheMock.Setup(c => c.RemoveAsync("all_venues", It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _venueService.CreateVenueAsync(dto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        capturedVenue.Should().NotBeNull();
        capturedVenue.Name.Should().Be("Premier Hall");
        capturedVenue.Rows.Should().Be(20);
        capturedVenue.Columns.Should().Be(30);
    }

    #endregion
}

/// <summary>
/// Helper extension method to build a mock DbSet from an IQueryable collection.
/// This allows us to easily mock Entity Framework DbSet queries in tests.
/// </summary>
public static class MockDbSetExtensions
{
    public static Mock<DbSet<T>> BuildMockDbSet<T>(this IQueryable<T> sourceList) where T : class
    {
        var mockDbSet = new Mock<DbSet<T>>();

        mockDbSet.As<IAsyncEnumerable<T>>()
            .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
            .Returns(new AsyncEnumerator<T>(sourceList.GetEnumerator()));

        mockDbSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(sourceList.Provider);
        mockDbSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(sourceList.Expression);
        mockDbSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(sourceList.ElementType);
        mockDbSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(sourceList.GetEnumerator());

        return mockDbSet;
    }
}

/// <summary>
/// Helper class to implement IAsyncEnumerator for mocking async DbSet queries.
/// </summary>
public class AsyncEnumerator<T> : IAsyncEnumerator<T>
{
    private readonly IEnumerator<T> _enumerator;

    public AsyncEnumerator(IEnumerator<T> enumerator) => _enumerator = enumerator;

    public T Current => _enumerator.Current;

    public ValueTask DisposeAsync() => default;

    public ValueTask<bool> MoveNextAsync() => new(_enumerator.MoveNext());
}
