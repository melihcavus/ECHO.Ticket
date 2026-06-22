using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using ECHO.Ticket.Business.Interfaces;
using ECHO.Ticket.Core.DTOs;
using ECHO.Ticket.Core.Entities;
using ECHO.Ticket.Core.Results;
using ECHO.Ticket.DataAccess.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed; // Redis arayüzü için gerekli kütüphane

namespace ECHO.Ticket.Business.Services;

public class VenueService : IVenueService
{
    private readonly EchoDbContext _context;
    private readonly IDistributedCache _cache; // Redis bağımlılığı

    public VenueService(EchoDbContext context, IDistributedCache cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<Result<IEnumerable<VenueDto>>> GetAllVenuesAsync()
    {
        const string cacheKey = "all_venues";

        // 1. ADIM: Veriyi Redis'ten (Önbellek) okumayı dene
        var cachedVenues = await _cache.GetStringAsync(cacheKey);

        if (!string.IsNullOrEmpty(cachedVenues))
        {
            // Cache Hit: Veri Redis'te bulundu! JSON'dan listeye çevirip hemen dönüyoruz.
            Console.WriteLine("🚀 DİKKAT: VERİLER POSTGRESQL'DEN DEĞİL, REDİS'TEN GELDİ!");
            var venuesFromCache = JsonSerializer.Deserialize<IEnumerable<VenueDto>>(cachedVenues);
            return Result<IEnumerable<VenueDto>>.Success(venuesFromCache);
        }

        // 2. ADIM: Cache Miss: Veri Redis'te yoksa, her zamanki gibi Veritabanından çek
        var venues = await _context.Venues
            .Select(v => new VenueDto
            {
                Id = v.Id,
                Name = v.Name,
                Rows = v.Rows,
                Columns = v.Columns
            }).ToListAsync();

        // 3. ADIM: Veritabanından alınan bu veriyi JSON'a çevirip Redis'e yaz (Örn: 24 Saatlik)
        var cacheOptions = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24)
        };
        
        var serializedVenues = JsonSerializer.Serialize(venues);
        await _cache.SetStringAsync(cacheKey, serializedVenues, cacheOptions);

        return Result<IEnumerable<VenueDto>>.Success(venues);
    }

    public async Task<Result> CreateVenueAsync(CreateVenueDto createVenueDto)
    {
        var venue = new Venue
        {
            Name = createVenueDto.Name,
            Rows = createVenueDto.Rows,
            Columns = createVenueDto.Columns
        };

        await _context.Venues.AddAsync(venue);
        await _context.SaveChangesAsync();

        // CACHE INVALIDATION: Yeni sahne eklendiği için Redis'teki eski listeyi çöpe atıyoruz.
        // Bir sonraki GetAll çağrısında güncel veri çekilip tekrar Redis'e yazılacak.
        await _cache.RemoveAsync("all_venues");

        return Result.Success("Sahne başarıyla oluşturuldu.");
    }
    public async Task<Result> DeleteVenueAsync(Guid id) // ID tipine dikkat et
    {
        var venue = await _context.Venues.FindAsync(id);
    
        if (venue == null)
            return Result.Failure("Silinmek istenen sahne bulunamadı.");

        _context.Venues.Remove(venue);
        await _context.SaveChangesAsync();

        // Sildikten sonra önbelleği temizliyoruz ki arayüzde de hemen kaybolsun
        await _cache.RemoveAsync("all_venues");

        return Result.Success("Sahne başarıyla silindi.");
    }
}