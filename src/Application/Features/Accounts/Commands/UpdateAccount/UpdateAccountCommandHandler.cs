using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Domain.Entities;
using MediatR;

namespace Application.Features.Accounts.Commands.UpdateAccount;

public class UpdateAccountCommandHandler : IRequestHandler<UpdateAccountCommand>
{
    private readonly IAccountRepository _accountRepository;
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public UpdateAccountCommandHandler(
        IAccountRepository accountRepository,
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _accountRepository = accountRepository;
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task Handle(UpdateAccountCommand request, CancellationToken cancellationToken)
    {
        var account = await _accountRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Account), request.Id);

        if (account.UserId != _currentUserService.UserId)
        {
            throw new ForbiddenAccessException();
        }

        account.Name = request.Name;
        account.Type = request.Type;

        _accountRepository.Update(account);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
