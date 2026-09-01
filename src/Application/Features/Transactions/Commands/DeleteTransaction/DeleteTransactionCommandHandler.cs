using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Domain.Entities;
using Domain.Enums;
using MediatR;

namespace Application.Features.Transactions.Commands.DeleteTransaction;

public class DeleteTransactionCommandHandler : IRequestHandler<DeleteTransactionCommand>
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public DeleteTransactionCommandHandler(
        ITransactionRepository transactionRepository,
        IAccountRepository accountRepository,
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _transactionRepository = transactionRepository;
        _accountRepository = accountRepository;
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task Handle(DeleteTransactionCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId ?? throw new ForbiddenAccessException();

        var transaction = await _transactionRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Transaction), request.Id);

        var account = await _accountRepository.GetByIdAsync(transaction.AccountId, cancellationToken)
            ?? throw new NotFoundException(nameof(Account), transaction.AccountId);

        if (account.UserId != userId)
        {
            throw new ForbiddenAccessException();
        }

        var signedAmount = transaction.Type == TransactionType.Income ? transaction.Amount : -transaction.Amount;
        account.Balance = account.Balance with { Amount = account.Balance.Amount - signedAmount };

        _transactionRepository.Remove(transaction);
        _accountRepository.Update(account);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
