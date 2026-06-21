using System;
using System.Linq;
using System.Threading.Tasks;
using ECHO.Ticket.Business.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECHO.Ticket.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class RecommendationController : ControllerBase
{
    private readonly IAiRecommendationService _aiService;
    private readonly IEventService _eventService;

    public RecommendationController(IAiRecommendationService aiService, IEventService eventService)
    {
        _aiService = aiService;
        _eventService = eventService;
    }

    [HttpGet("personal-picks/{userId}")]
    public async Task<IActionResult> GetPersonalizedRecommendations(string userId)
    {
        // 1. ADIM: Veritabanındaki tüm etkinlikleri getir
        var eventsResult = await _eventService.GetAllEventsAsync(); 
        
        if (!eventsResult.IsSuccess || eventsResult.Data == null || !eventsResult.Data.Any())
            return BadRequest(new { isSuccess = false, message = "Önerilecek etkinlik bulunamadı." });

        var availableEvents = eventsResult.Data.ToList();

        // 2. ADIM: JÜRİYİ BÜYÜLEYECEK KISIM (PARALEL İŞLEME)
        // Tüm etkinlikleri aynı anda yapay zekaya soruyoruz (Performans patlaması)
        var predictionTasks = availableEvents.Select(async ev => 
        {
            // Python API'mize (8001 portuna) Kullanıcı, Kategori ve Lokasyon bilgilerini atıyoruz
            double score = await _aiService.GetEventScorePredictionAsync(
                userId: userId, 
                category: ev.Category, // Örn: "Teknoloji"
                location: ev.Location  // Örn: "İstanbul"
            );

            return new 
            {
                EventData = ev,
                AiScore = score
            };
        });

        // Tüm yapay zeka isteklerinin bitmesini bekle
        var scoredEvents = await Task.WhenAll(predictionTasks);

        // 3. ADIM: Puanı en yüksek (AI'ın en çok beğendiği) ilk 4 etkinliği seç
        var topRecommendations = scoredEvents
            .OrderByDescending(x => x.AiScore)
            .Take(4)
            .Select(x => x.EventData) 
            .ToList();

        return Ok(new { isSuccess = true, data = topRecommendations });
    }
}