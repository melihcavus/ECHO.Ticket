using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ECHO.Ticket.Business.Interfaces;
using ECHO.Ticket.Core.DTOs;
using ECHO.Ticket.Core.Entities;
using ECHO.Ticket.Core.Results;
using ECHO.Ticket.DataAccess.Interfaces;
using Mapster;

namespace ECHO.Ticket.Business.Services;

public class EventReviewService : IEventReviewService
{
    private readonly IRepository<EventReview> _reviewRepository;
    private readonly IRepository<User> _userRepository;
    private readonly IWorkContext _workContext;
    private readonly ISentimentAnalysisService _sentimentService;

    public EventReviewService(
        IRepository<EventReview> reviewRepository, 
        IRepository<User> userRepository, 
        IWorkContext workContext,
        ISentimentAnalysisService sentimentService)
    {
        _reviewRepository = reviewRepository;
        _userRepository = userRepository;
        _workContext = workContext;
        _sentimentService = sentimentService;
    }

    public async Task<Result> AddReviewAsync(EventReviewCreateDto reviewDto)
    {
        if (reviewDto.Rating < 1 || reviewDto.Rating > 5)
            return Result.Failure("Hata: Puanlama 1 ile 5 yıldız arasında olmalıdır.");

        var userId = _workContext.UserId;

        var newReview = reviewDto.Adapt<EventReview>();
        newReview.UserId = userId;
        newReview.CreatedAt = DateTime.UtcNow;

        // Yapay zeka analizi dış bir servise devrediliyor (SRP)
        var aiResult = await _sentimentService.AnalyzeReviewAsync(reviewDto.Content);
        
        newReview.SentimentLabel = aiResult.Label;
        newReview.SentimentScore = aiResult.Score;

        await _reviewRepository.AddAsync(newReview);
        await _reviewRepository.SaveChangesAsync();

        return Result.Success("Yorumunuz başarıyla kaydedildi ve yapay zeka tarafından analiz edildi.");
    }

    public async Task<Result<IEnumerable<EventReviewDto>>> GetReviewsByEventIdAsync(Guid eventId)
    {
        // PERFORMANS ÇÖZÜMÜ: Sadece bu etkinliğe ait yorumları veritabanından çeker
        var eventReviews = (await _reviewRepository.FindAsync(r => r.EventId == eventId)).ToList();

        var userIds = eventReviews.Select(r => r.UserId).Distinct().ToList();
        
        // Kullanıcıları çekerken de filtreleme uygulayabiliriz (Opsiyonel ama daha iyi)
        var reviewUsers = (await _userRepository.FindAsync(u => userIds.Contains(u.Id))).ToList();

        var reviewDtos = eventReviews.Select(r => 
        {
            var user = reviewUsers.FirstOrDefault(u => u.Id == r.UserId);
            
            return new EventReviewDto
            {
                Id = r.Id,
                UserId = r.UserId,
                UserFullName = user != null ? $"{user.FirstName} {user.LastName}" : "ECHO Kullanıcısı",
                Rating = r.Rating,
                Content = r.Content,
                CreatedAt = r.CreatedAt,
                SentimentLabel = r.SentimentLabel,
                SentimentScore = r.SentimentScore
            };
        }).OrderByDescending(r => r.CreatedAt);

        return Result<IEnumerable<EventReviewDto>>.Success(reviewDtos);
    }

    public async Task<Result<EventAnalyticsDto>> GetEventAnalyticsAsync(Guid eventId)
    {
        // PERFORMANS ÇÖZÜMÜ: Tüm tabloyu çekmek yerine sadece ilgili etkinliğin yorumlarını al
        var eventReviews = (await _reviewRepository.FindAsync(r => r.EventId == eventId)).ToList();

        if (!eventReviews.Any())
        {
            return Result<EventAnalyticsDto>.Success(new EventAnalyticsDto
            {
                EventId = eventId,
                SatisfactionScore = 0
            }, "Bu etkinlik için henüz yorum yapılmamış.");
        }

        int total = eventReviews.Count;
        double threshold = 80.0; 

        int positive = eventReviews.Count(r => 
            r.SentimentLabel?.Equals("POSITIVE", StringComparison.OrdinalIgnoreCase) == true && 
            r.SentimentScore >= threshold);

        int negative = eventReviews.Count(r => 
            r.SentimentLabel?.Equals("NEGATIVE", StringComparison.OrdinalIgnoreCase) == true && 
            r.SentimentScore >= threshold);

        int neutral = total - positive - negative;

        double satisfaction = total > 0 ? Math.Round((double)positive / total * 100, 2) : 0;

        var criticalEntities = eventReviews
            .Where(r => r.SentimentLabel?.Equals("NEGATIVE", StringComparison.OrdinalIgnoreCase) == true)
            .OrderByDescending(r => r.SentimentScore)
            .Take(5)
            .ToList();

        var userIds = criticalEntities.Select(r => r.UserId).Distinct().ToList();
        var reviewUsers = (await _userRepository.FindAsync(u => userIds.Contains(u.Id))).ToList();

        var criticalDtos = criticalEntities.Select(r => 
        {
            var user = reviewUsers.FirstOrDefault(u => u.Id == r.UserId);
            return new EventReviewDto
            {
                Id = r.Id,
                UserId = r.UserId,
                UserFullName = user != null ? $"{user.FirstName} {user.LastName}" : "ECHO Kullanıcısı",
                Rating = r.Rating,
                Content = r.Content,
                CreatedAt = r.CreatedAt,
                SentimentLabel = r.SentimentLabel,
                // BUG FIX: Zaten 100 üzerinden olduğu için bir daha çarpmıyoruz.
                SentimentScore = r.SentimentScore != null ? Math.Round(r.SentimentScore.Value, 2) : 0
            };
        }).ToList();

        var analytics = new EventAnalyticsDto
        {
            EventId = eventId,
            TotalReviews = total,
            PositiveCount = positive,
            NegativeCount = negative,
            NeutralCount = neutral,
            SatisfactionScore = satisfaction,
            CriticalReviews = criticalDtos
        };

        return Result<EventAnalyticsDto>.Success(analytics);
    }
}