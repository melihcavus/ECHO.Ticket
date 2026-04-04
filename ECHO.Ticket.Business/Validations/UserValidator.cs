using FluentValidation;
using UserEntity = ECHO.Ticket.Core.Entities.User;

namespace ECHO.Ticket.Business.Validations;

public class UserValidator : AbstractValidator<UserEntity>
{
    public UserValidator()
    {
        RuleFor(u => u.Email)
            .NotEmpty().WithMessage("E-posta adresi boş bırakılamaz.")
            .EmailAddress().WithMessage("Lütfen geçerli bir e-posta adresi giriniz.");
    }
}