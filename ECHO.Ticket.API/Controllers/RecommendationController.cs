using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ECHO.Ticket.Business.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ECHO.Ticket.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class RecommendationController : ControllerBase
{
    private readonly IAiRecommendationService _aiService;
    private readonly IEventService _eventService; // Etkinlikleri veritabanından çekmek için

    public RecommendationController(IAiRecommendationService aiService, IEventService eventService)
    {
        _aiService = aiService;
        _eventService = eventService;
    }

    [HttpGet("ForUser/{userId}")]
    public async Task<IActionResult> GetRecommendations(string userId)
    {
        // 1. ADIM: Veritabanındaki tüm etkinlikleri (veya aktif kampanyaları) getir
        // Not: Kendi IEventService'indeki metodun adını (örneğin GetAllAsync) buraya yazmalısın.
        var eventsResult = await _eventService.GetAllEventsAsync(); 
        
        if (!eventsResult.IsSuccess)
            return BadRequest(eventsResult.Message);

        var scoredEvents = new List<dynamic>();

        // 2. ADIM: Her bir etkinlik için Python Yapay Zeka sunucumuza puan sor
        foreach (var ev in eventsResult.Data)
        {
            // Python'da modeli eğitirken kategorileri sayılara (0, 1, 2, 3) çevirmiştik.
            // Bu yüzden C#'tan gönderirken de metin yerine o sayı kodunu bulup gönderiyoruz.
            int categoryCode = GetCategoryCode(ev.Category); // DTO'ndaki kategori property'sine göre burayı uyarla
            
            int numericUserId = Math.Abs(userId.GetHashCode());
            // Zekamızdan tahmini puanı (Örn: 4.85) alıyoruz
            double predictedScore = await _aiService.GetEventScorePredictionAsync(numericUserId, ev.Id, categoryCode);

            scoredEvents.Add(new
            {
                EventData = ev,
                AiScore = predictedScore
            });
        }

        // 3. ADIM: Puanı en yüksek (Yapay Zekanın en çok uygun gördüğü) ilk 4 etkinliği seç
        var topRecommendations = scoredEvents
            .OrderByDescending(x => x.AiScore)
            .Take(4)
            .Select(x => x.EventData) // Sadece etkinlik verisini Frontend'e dönüyoruz
            .ToList();

        return Ok(topRecommendations);
    }

    // Makine öğrenmesi metin anlamaz, sayılarla çalışır. Bu yüzden eşleştirme yapıyoruz.
    private int GetCategoryCode(string categoryName)
    {
        return categoryName switch
        {
            "Bilişim ve Teknoloji" => 0,
            "Eğitim" => 1,
            "Sanat ve Tasarım" => 2,
            "Spor" => 3,
            _ => 0
        };
    }
}