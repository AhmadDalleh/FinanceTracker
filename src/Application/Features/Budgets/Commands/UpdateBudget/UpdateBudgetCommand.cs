using MediatR;

namespace Application.Features.Budgets.Commands.UpdateBudget;

public record UpdateBudgetCommand : IRequest
{
    public Guid Id { get; init; }
    public decimal Amount { get; init; }
}
