using Application.Common.Validation;
using FluentValidation;

namespace Application.Features.Transactions.Commands.UpdateTransaction;

public class UpdateTransactionCommandValidator : AbstractValidator<UpdateTransactionCommand>
{
    public UpdateTransactionCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();

        RuleFor(x => x.CategoryId)
            .NotEmpty();

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("Amount must be positive; the transaction type determines whether it is a debit or credit.")
            .LessThanOrEqualTo(MoneyLimits.MaxAmount)
            .WithMessage($"Amount cannot exceed {MoneyLimits.MaxAmount:N2}.")
            .Must(MoneyLimits.HasAtMostTwoDecimalPlaces)
            .WithMessage("Amount cannot have more than 2 decimal places.");

        RuleFor(x => x.Type)
            .IsInEnum();

        RuleFor(x => x.Date)
            .NotEqual(default(DateOnly));

        RuleFor(x => x.Note)
            .MaximumLength(500);
    }
}
