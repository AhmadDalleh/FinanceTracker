using Domain.Enums;
using MediatR;

namespace Application.Features.Accounts.Commands.UpdateAccount;

public record UpdateAccountCommand : IRequest
{
    public Guid Id { get; init; }
    public required string Name { get; init; }
    public AccountType Type { get; init; }
}
