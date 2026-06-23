using System;
using System.Net.Http;
using System.Net.Http.Headers;
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
        
        // Ana Adresimiz (Bu gizli bir bilgi değil, kalsın)
        var aiUrl = "https://melih11-echo-ai-sentiment.hf.space/" ?? "http://127.0.0.1:8000/";
        
        // DİKKAT: Token'ı koddan sildik! Artık IConfiguration üzerinden Render'daki gizli kasadan çekecek.
        var token = _configuration["HuggingFaceToken"]; 
        
        _httpClient.BaseAddress = new Uri(aiUrl); 
        
        if (!string.IsNullOrEmpty(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }

    public async Task<(string Label, double Score)> AnalyzeReviewAsync(string reviewText)
    {
        try
        {
            var payload = new { review_text = reviewText };
            var jsonPayload = JsonSerializer.Serialize(payload);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("analyze", content);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(responseContent);
                
                string label = doc.RootElement.GetProperty("sentiment").GetString();
                double score = doc.RootElement.GetProperty("confidence_score").GetDouble();
                
                return (label, score);
            }
            else
            {
                Console.WriteLine($"Yapay Zeka API Hatası: Sunucu {response.StatusCode} kodu döndürdü.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Yapay Zeka Bağlantı Hatası: {ex.Message}");
        }

        return ("NEUTRAL", 0); 
    }
}