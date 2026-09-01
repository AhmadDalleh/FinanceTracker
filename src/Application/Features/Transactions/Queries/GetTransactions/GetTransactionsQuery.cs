using Application.Common.Models;
using MediatR;

namespace Application.Features.Transactions.Queries.GetTransactions;

public record GetTransactionsQuery : IRequest<PaginatedList<TransactionDto>>
{
    public Guid? AccountId { get; init; }
    public Guid? CategoryId { get; init; }
    public DateOnly? FromDate { get; init; }
    public DateOnly? ToDate { get; init; }
    public decimal? MinAmount { get; init; }
    public decimal? MaxAmount { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}
