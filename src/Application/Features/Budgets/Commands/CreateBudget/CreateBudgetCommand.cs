using MediatR;

namespace Application.Features.Budgets.Commands.CreateBudget;

public record CreateBudgetCommand : IRequest<Guid>
{
    public Guid CategoryId { get; init; }
    public int Year { get; init; }
    public int Month { get; init; }
    public decimal Amount { get; init; }
}
