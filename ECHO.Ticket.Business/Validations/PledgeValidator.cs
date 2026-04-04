using FluentValidation;
using PledgeEntity = ECHO.Ticket.Core.Entities.Pledge;

namespace ECHO.Ticket.Business.Validations;

public class PledgeValidator : AbstractValidator<PledgeEntity>
{
    public PledgeValidator()
    {
        RuleFor(p => p.UserId)
            .NotEmpty().WithMessage("Destek yapan kullanıcı ID'si boş olamaz.");
            
        RuleFor(p => p.TicketId)
            .NotEmpty().WithMessage("Satın alınan bilet ID'si boş olamaz.");
    }
}