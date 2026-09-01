using Domain.Enums;
using MediatR;

namespace Application.Features.Transactions.Commands.CreateTransaction;

public record CreateTransactionCommand : IRequest<Guid>
{
    public Guid AccountId { get; init; }
    public Guid CategoryId { get; init; }
    public decimal Amount { get; init; }
    public TransactionType Type { get; init; }
    public DateOnly Date { get; init; }
    public string? Note { get; init; }
}
