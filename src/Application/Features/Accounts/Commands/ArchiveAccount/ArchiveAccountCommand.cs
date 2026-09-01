using MediatR;

namespace Application.Features.Accounts.Commands.ArchiveAccount;

public record ArchiveAccountCommand(Guid Id) : IRequest;
