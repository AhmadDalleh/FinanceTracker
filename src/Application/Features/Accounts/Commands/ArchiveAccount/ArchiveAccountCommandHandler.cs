using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Domain.Entities;
using MediatR;

namespace Application.Features.Accounts.Commands.ArchiveAccount;

public class ArchiveAccountCommandHandler : IRequestHandler<ArchiveAccountCommand>
{
    private readonly IAccountRepository _accountRepository;
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public ArchiveAccountCommandHandler(
        IAccountRepository accountRepository,
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _accountRepository = accountRepository;
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task Handle(ArchiveAccountCommand request, CancellationToken cancellationToken)
    {
        var account = await _accountRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Account), request.Id);

        if (account.UserId != _currentUserService.UserId)
        {
            throw new ForbiddenAccessException();
        }

        account.IsArchived = true;

        _accountRepository.Update(account);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
