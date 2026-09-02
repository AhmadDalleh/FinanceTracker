using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Domain.Entities;
using MediatR;
using ValidationException = Application.Common.Exceptions.ValidationException;

namespace Application.Features.Budgets.Commands.CreateBudget;

public class CreateBudgetCommandHandler : IRequestHandler<CreateBudgetCommand, Guid>
{
    private readonly IBudgetRepository _budgetRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public CreateBudgetCommandHandler(
        IBudgetRepository budgetRepository,
        ICategoryRepository categoryRepository,
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _budgetRepository = budgetRepository;
        _categoryRepository = categoryRepository;
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Guid> Handle(CreateBudgetCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId ?? throw new ForbiddenAccessException();

        var categoryExists = await _categoryRepository.ExistsAsync(request.CategoryId, userId, cancellationToken);
        if (!categoryExists)
        {
            throw new NotFoundException(nameof(Category), request.CategoryId);
        }

        var month = new DateOnly(request.Year, request.Month, 1);

        var duplicateExists = await _budgetRepository.ExistsForCategoryAndMonthAsync(request.CategoryId, month, userId, cancellationToken);
        if (duplicateExists)
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.CategoryId), "A budget for this category and month already exists.")
            });
        }

        var budget = new Budget
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CategoryId = request.CategoryId,
            Month = month,
            Amount = request.Amount
        };

        await _budgetRepository.AddAsync(budget, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return budget.Id;
    }
}
