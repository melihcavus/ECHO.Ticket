using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using ECHO.Ticket.Business.Interfaces;

namespace ECHO.Ticket.Business.Services;

public class SentimentAnalysisService : ISentimentAnalysisService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public SentimentAnalysisService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        
        // Render panelinden linki okur, bulamazsa senin bilgisayarındaki 8000 portunu kullanır.
        var aiUrl = _configuration["AIServices__SentimentUrl"] ?? "http://127.0.0.1:8000/";
        
        // HttpClient BaseAddress sonuna mutlaka '/' ister.
        if (!aiUrl.EndsWith("/")) aiUrl += "/";
        
        _httpClient.BaseAddress = new Uri(aiUrl); 
    }

    public async Task<(string Label, double Score)> AnalyzeReviewAsync(string reviewText)
    {
        try
        {
            // Python'un beklediği JSON formatı
            var payload = new { review_text = reviewText };
            var jsonPayload = JsonSerializer.Serialize(payload);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            // Python API'ye isteği atıyoruz
            var response = await _httpClient.PostAsync("analyze", content);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(responseContent);
                
                // Python'dan dönen "sentiment" ve "confidence_score" değerlerini yakalıyoruz
                string label = doc.RootElement.GetProperty("sentiment").GetString();
                double score = doc.RootElement.GetProperty("confidence_score").GetDouble();
                
                return (label, score);
            }
        }
        catch (Exception ex)
        {
            // Eğer Python sunucusu kapalıysa C# çökmesin diye varsayılan değer dönüyoruz
            Console.WriteLine($"Yapay Zeka Bağlantı Hatası: {ex.Message}");
        }

        return ("NEUTRAL", 0); // Python'a ulaşılamazsa veya hata olursa
    }
}