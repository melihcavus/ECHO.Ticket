using System.Threading.Tasks;

namespace ECHO.Ticket.Business.Interfaces;

public interface IAiRecommendationService
{
    Task<double> GetEventScorePredictionAsync(string userId, string category, string location);
}