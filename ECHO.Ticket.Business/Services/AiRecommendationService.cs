using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using ECHO.Ticket.Business.Interfaces;

namespace ECHO.Ticket.Business.Services;

public class AiRecommendationService : IAiRecommendationService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public AiRecommendationService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        
        // Render panelinden linki okur, bulamazsa senin bilgisayarındaki 8001 portunu kullanır.
        var aiUrl = _configuration["AIServices__RecommendationUrl"] ?? "http://127.0.0.1:8001/";
        
        // HttpClient BaseAddress sonuna mutlaka '/' ister, hata vermemesi için güvenceye alıyoruz.
        if (!aiUrl.EndsWith("/")) aiUrl += "/";
        
        _httpClient.BaseAddress = new Uri(aiUrl); 
    }

    public async Task<double> GetEventScorePredictionAsync(string userId, string category, string location)
    {
        try
        {
            // Python FastAPI'nin beklediği JSON şablonu (RecommendationRequest)
            var payload = new 
            { 
                user_id = userId, 
                category = category, 
                location = location 
            };
            
            var jsonPayload = JsonSerializer.Serialize(payload);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            // Python'a POST isteği atıyoruz
            var response = await _httpClient.PostAsync("predict-score", content);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(responseContent);
                
                // Python'dan dönen "predicted_score" değerini yakalıyoruz
                return doc.RootElement.GetProperty("predicted_score").GetDouble();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Yapay Zeka (Öneri Motoru) Bağlantı Hatası: {ex.Message}");
        }

        // Hata olursa veya Python sunucusu kapalıysa sistemi çökertmek yerine Nötr (3.0) dönüyoruz.
        return 3.0; 
    }
}