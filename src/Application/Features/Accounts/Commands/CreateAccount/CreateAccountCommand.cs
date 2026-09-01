using Domain.Enums;
using MediatR;

namespace Application.Features.Accounts.Commands.CreateAccount;

public record CreateAccountCommand : IRequest<Guid>
{
    public required string Name { get; init; }
    public AccountType Type { get; init; }
    public decimal StartingBalance { get; init; }
    public required string Currency { get; init; }
}
