using Application.Common.Validation;
using FluentValidation;

namespace Application.Features.Budgets.Commands.UpdateBudget;

public class UpdateBudgetCommandValidator : AbstractValidator<UpdateBudgetCommand>
{
    public UpdateBudgetCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("Budgeted amount must be positive.")
            .LessThanOrEqualTo(MoneyLimits.MaxAmount)
            .WithMessage($"Budgeted amount cannot exceed {MoneyLimits.MaxAmount:N2}.")
            .Must(MoneyLimits.HasAtMostTwoDecimalPlaces)
            .WithMessage("Budgeted amount cannot have more than 2 decimal places.");
    }
}
