using ECHO.Ticket.Core.Entities;
using FluentValidation;

namespace ECHO.Ticket.Business.Validations;

// AbstractValidator'dan miras alıyoruz ve hangi sınıfı doğrulayacağını (Event) söylüyoruz.
public class EventValidator : AbstractValidator<Event>
{
    public EventValidator()
    {
        // 1. Etkinlik adı boş olamaz ve en az 3 karakter olmalı
        RuleFor(e => e.Title) 
            .NotEmpty().WithMessage("Etkinlik adı boş bırakılamaz.")
            .MinimumLength(3).WithMessage("Etkinlik adı en az 3 karakter olmalıdır.");

        //Tarih geçmiş olamaz!
        RuleFor(e => e.EventDate) 
            .GreaterThan(DateTime.UtcNow).WithMessage("Etkinlik tarihi geçmiş bir tarih olamaz.");

        // İstersen kapasite veya lokasyon kuralları da ekleyebilirsin:
        // RuleFor(e => e.Location).NotEmpty().WithMessage("Lokasyon belirtilmelidir.");
    }
}