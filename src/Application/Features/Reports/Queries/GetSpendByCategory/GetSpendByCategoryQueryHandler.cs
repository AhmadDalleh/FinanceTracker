using Application.Common.Extensions;
using Application.Common.Interfaces;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Reports.Queries.GetSpendByCategory;

public class GetSpendByCategoryQueryHandler : IRequestHandler<GetSpendByCategoryQuery, List<CategorySpendDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetSpendByCategoryQueryHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<List<CategorySpendDto>> Handle(GetSpendByCategoryQuery request, CancellationToken cancellationToken)
    {
        var monthStart = new DateOnly(request.Year, request.Month, 1);
        var monthEnd = monthStart.AddMonths(1).AddDays(-1);

        var ownedAccountIds = _context.OwnedAccountIds(_currentUserService.UserId);

        var spend = await _context.Transactions
            .Where(t =>
                ownedAccountIds.Contains(t.AccountId) &&
                t.Type == TransactionType.Expense &&
                t.Date >= monthStart &&
                t.Date <= monthEnd)
            .GroupBy(t => t.CategoryId)
            .Select(g => new { CategoryId = g.Key, Total = g.Sum(t => t.Amount) })
            .ToListAsync(cancellationToken);

        if (spend.Count == 0)
        {
            return [];
        }

        var categoryIds = spend.Select(s => s.CategoryId).ToList();
        var categoryNames = await _context.Categories
            .Where(c => categoryIds.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id, c => c.Name, cancellationToken);

        return spend
            .Select(s => new CategorySpendDto
            {
                CategoryId = s.CategoryId,
                CategoryName = categoryNames.GetValueOrDefault(s.CategoryId, string.Empty),
                TotalSpent = s.Total
            })
            .OrderByDescending(s => s.TotalSpent)
            .ToList();
    }
}
