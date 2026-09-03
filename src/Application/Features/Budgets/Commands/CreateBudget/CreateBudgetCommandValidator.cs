using Application.Common.Validation;
using FluentValidation;

namespace Application.Features.Budgets.Commands.CreateBudget;

public class CreateBudgetCommandValidator : AbstractValidator<CreateBudgetCommand>
{
    public CreateBudgetCommandValidator()
    {
        RuleFor(x => x.CategoryId)
            .NotEmpty();

        RuleFor(x => x.Year)
            .InclusiveBetween(2000, 2100);

        RuleFor(x => x.Month)
            .InclusiveBetween(1, 12);

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("Budgeted amount must be positive.")
            .LessThanOrEqualTo(MoneyLimits.MaxAmount)
            .WithMessage($"Budgeted amount cannot exceed {MoneyLimits.MaxAmount:N2}.")
            .Must(MoneyLimits.HasAtMostTwoDecimalPlaces)
            .WithMessage("Budgeted amount cannot have more than 2 decimal places.");
    }
}
