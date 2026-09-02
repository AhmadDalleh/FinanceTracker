using FluentValidation;

namespace Application.Features.Auth.Commands.Register;

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(256);

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8)
            .Must(p => p.Any(char.IsLetter))
            .WithMessage("Password must contain at least one letter.")
            .Must(p => p.Any(char.IsDigit))
            .WithMessage("Password must contain at least one digit.");
    }
}
