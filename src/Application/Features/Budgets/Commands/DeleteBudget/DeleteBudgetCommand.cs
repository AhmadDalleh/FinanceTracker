using MediatR;

namespace Application.Features.Budgets.Commands.DeleteBudget;

public record DeleteBudgetCommand(Guid Id) : IRequest;
