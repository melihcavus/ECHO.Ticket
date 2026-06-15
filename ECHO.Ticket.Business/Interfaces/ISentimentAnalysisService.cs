using System.Threading.Tasks;

namespace ECHO.Ticket.Business.Interfaces;

public interface ISentimentAnalysisService
{
    // C# Python'a metni gönderecek, Python ise bize (Pozitif/Negatif, %Skor) dönecek
    Task<(string Label, double Score)> AnalyzeReviewAsync(string reviewText);
}