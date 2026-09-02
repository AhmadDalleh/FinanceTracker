using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Domain.Entities;
using MediatR;

namespace Application.Features.Budgets.Commands.UpdateBudget;

public class UpdateBudgetCommandHandler : IRequestHandler<UpdateBudgetCommand>
{
    private readonly IBudgetRepository _budgetRepository;
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public UpdateBudgetCommandHandler(
        IBudgetRepository budgetRepository,
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _budgetRepository = budgetRepository;
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task Handle(UpdateBudgetCommand request, CancellationToken cancellationToken)
    {
        var budget = await _budgetRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Budget), request.Id);

        if (budget.UserId != _currentUserService.UserId)
        {
            throw new ForbiddenAccessException();
        }

        budget.Amount = request.Amount;

        _budgetRepository.Update(budget);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
