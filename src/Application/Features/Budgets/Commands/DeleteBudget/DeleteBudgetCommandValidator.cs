using FluentValidation;

namespace Application.Features.Budgets.Commands.DeleteBudget;

public class DeleteBudgetCommandValidator : AbstractValidator<DeleteBudgetCommand>
{
    public DeleteBudgetCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();
    }
}
