using MediatR;

namespace Application.Features.Budgets.Queries.GetBudgets;

public record GetBudgetsQuery(int Year, int Month) : IRequest<List<BudgetDto>>;
