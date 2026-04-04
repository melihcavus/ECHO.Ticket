using FluentValidation;
using TicketEntity = ECHO.Ticket.Core.Entities.Ticket;

namespace ECHO.Ticket.Business.Validations;

public class TicketValidator : AbstractValidator<TicketEntity>
{
    public TicketValidator()
    {
        RuleFor(t => t.Price)
            .GreaterThanOrEqualTo(0).WithMessage("Bilet/Paket fiyatı 0'dan küçük olamaz.");
            
        RuleFor(t => t.EventId)
            .NotEmpty().WithMessage("Biletin bağlanacağı etkinlik ID'si boş olamaz.");
        
        RuleFor(t => t.Capacity).GreaterThan(0).WithMessage("Kapasite en az 1 olmalıdır.");
    }
}