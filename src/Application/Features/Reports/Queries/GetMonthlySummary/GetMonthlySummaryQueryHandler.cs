using Application.Common.Extensions;
using Application.Common.Interfaces;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Reports.Queries.GetMonthlySummary;

public class GetMonthlySummaryQueryHandler : IRequestHandler<GetMonthlySummaryQuery, MonthlySummaryDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetMonthlySummaryQueryHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<MonthlySummaryDto> Handle(GetMonthlySummaryQuery request, CancellationToken cancellationToken)
    {
        var monthStart = new DateOnly(request.Year, request.Month, 1);
        var monthEnd = monthStart.AddMonths(1).AddDays(-1);

        var ownedAccountIds = _context.OwnedAccountIds(_currentUserService.UserId);

        var totals = await _context.Transactions
            .Where(t => ownedAccountIds.Contains(t.AccountId) && t.Date >= monthStart && t.Date <= monthEnd)
            .GroupBy(t => t.Type)
            .Select(g => new { Type = g.Key, Total = g.Sum(t => t.Amount) })
            .ToListAsync(cancellationToken);

        var totalIncome = totals.FirstOrDefault(t => t.Type == TransactionType.Income)?.Total ?? 0m;
        var totalExpense = totals.FirstOrDefault(t => t.Type == TransactionType.Expense)?.Total ?? 0m;

        return new MonthlySummaryDto
        {
            Year = request.Year,
            Month = request.Month,
            TotalIncome = totalIncome,
            TotalExpense = totalExpense,
            NetPosition = totalIncome - totalExpense
        };
    }
}
