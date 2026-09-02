using Application.Common.Interfaces;
using AutoMapper;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Budgets.Queries.GetBudgets;

public class GetBudgetsQueryHandler : IRequestHandler<GetBudgetsQuery, List<BudgetDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;

    public GetBudgetsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUserService, IMapper mapper)
    {
        _context = context;
        _currentUserService = currentUserService;
        _mapper = mapper;
    }

    public async Task<List<BudgetDto>> Handle(GetBudgetsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        var month = new DateOnly(request.Year, request.Month, 1);
        var monthEnd = month.AddMonths(1).AddDays(-1);

        var budgets = await _context.Budgets
            .Where(b => b.UserId == userId && b.Month == month)
            .OrderBy(b => b.CategoryId)
            .ToListAsync(cancellationToken);

        if (budgets.Count == 0)
        {
            return [];
        }

        var categoryIds = budgets.Select(b => b.CategoryId).Distinct().ToList();

        var categoryNames = await _context.Categories
            .Where(c => categoryIds.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id, c => c.Name, cancellationToken);

        var spentByCategory = await _context.Transactions
            .Where(t =>
                categoryIds.Contains(t.CategoryId) &&
                t.Type == TransactionType.Expense &&
                t.Date >= month &&
                t.Date <= monthEnd)
            .GroupBy(t => t.CategoryId)
            .Select(g => new { CategoryId = g.Key, Total = g.Sum(t => t.Amount) })
            .ToDictionaryAsync(x => x.CategoryId, x => x.Total, cancellationToken);

        var dtos = _mapper.Map<List<BudgetDto>>(budgets);
        foreach (var dto in dtos)
        {
            dto.CategoryName = categoryNames.GetValueOrDefault(dto.CategoryId, string.Empty);
            dto.ActualSpent = spentByCategory.GetValueOrDefault(dto.CategoryId, 0m);
            dto.PercentageUsed = dto.BudgetedAmount == 0 ? 0 : Math.Round(dto.ActualSpent / dto.BudgetedAmount * 100, 1);
        }

        return dtos;
    }
}
