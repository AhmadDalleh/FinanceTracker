using Application.Common.Validation;
using FluentValidation;

namespace Application.Features.Accounts.Commands.CreateAccount;

public class CreateAccountCommandValidator : AbstractValidator<CreateAccountCommand>
{
    public CreateAccountCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Type)
            .IsInEnum();

        RuleFor(x => x.Currency)
            .NotEmpty()
            .Length(3)
            .WithMessage("Currency must be a 3-letter ISO code (e.g. USD).");

        RuleFor(x => x.StartingBalance)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Starting balance cannot be negative. Use a credit card account for existing debt.")
            .LessThanOrEqualTo(MoneyLimits.MaxAmount)
            .WithMessage($"Starting balance cannot exceed {MoneyLimits.MaxAmount:N2}.")
            .Must(MoneyLimits.HasAtMostTwoDecimalPlaces)
            .WithMessage("Starting balance cannot have more than 2 decimal places.");
    }
}
