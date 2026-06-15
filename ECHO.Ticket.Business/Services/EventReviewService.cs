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
    private readonly IRepository<User> _userRepository; // YENİ: Kullanıcıları çekmek için depo
    private readonly IWorkContext _workContext;

    // Constructor güncellendi
    public EventReviewService(
        IRepository<EventReview> reviewRepository, 
        IRepository<User> userRepository, 
        IWorkContext workContext)
    {
        _reviewRepository = reviewRepository;
        _userRepository = userRepository;
        _workContext = workContext;
    }

    public async Task<Result> AddReviewAsync(EventReviewCreateDto reviewDto)
    {
        if (reviewDto.Rating < 1 || reviewDto.Rating > 5)
            return Result.Failure("Hata: Puanlama 1 ile 5 yıldız arasında olmalıdır.");

        var userId = _workContext.UserId;

        var newReview = reviewDto.Adapt<EventReview>();
        newReview.UserId = userId;
        newReview.CreatedAt = DateTime.UtcNow;

        await _reviewRepository.AddAsync(newReview);
        await _reviewRepository.SaveChangesAsync();

        return Result.Success("Yorumunuz ve puanınız başarıyla kaydedildi.");
    }

    public async Task<Result<IEnumerable<EventReviewDto>>> GetReviewsByEventIdAsync(Guid eventId)
    {
        // 1. Tüm yorumları çek
        var allReviews = await _reviewRepository.GetAllAsync();
        var eventReviews = allReviews.Where(r => r.EventId == eventId).ToList();

        // 2. Yorum yapan kullanıcıların ID'lerini bul ve o kullanıcıları veritabanından çek
        var userIds = eventReviews.Select(r => r.UserId).Distinct().ToList();
        var allUsers = await _userRepository.GetAllAsync();
        var reviewUsers = allUsers.Where(u => userIds.Contains(u.Id)).ToList();

        // 3. Yorumlarla kullanıcı isimlerini birleştirerek DTO'yu oluştur
        var reviewDtos = eventReviews.Select(r => 
        {
            var user = reviewUsers.FirstOrDefault(u => u.Id == r.UserId);
            
            return new EventReviewDto
            {
                Id = r.Id,
                UserId = r.UserId,
                UserFullName = user != null ? $"{user.FirstName} {user.LastName}" : "ECHO Kullanıcısı", // ARTK GERÇEK İSİM GELECEK!
                Rating = r.Rating,
                Content = r.Content,
                CreatedAt = r.CreatedAt
            };
        }).OrderByDescending(r => r.CreatedAt); // En yeni yorumlar en üstte görünsün

        return Result<IEnumerable<EventReviewDto>>.Success(reviewDtos);
    }
}