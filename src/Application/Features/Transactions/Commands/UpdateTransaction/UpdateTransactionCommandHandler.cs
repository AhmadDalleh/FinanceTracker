using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Domain.Entities;
using Domain.Enums;
using MediatR;

namespace Application.Features.Transactions.Commands.UpdateTransaction;

public class UpdateTransactionCommandHandler : IRequestHandler<UpdateTransactionCommand>
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public UpdateTransactionCommandHandler(
        ITransactionRepository transactionRepository,
        IAccountRepository accountRepository,
        ICategoryRepository categoryRepository,
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _transactionRepository = transactionRepository;
        _accountRepository = accountRepository;
        _categoryRepository = categoryRepository;
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task Handle(UpdateTransactionCommand request, CancellationToken cancellationToken)
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

        var categoryExists = await _categoryRepository.ExistsAsync(request.CategoryId, userId, cancellationToken);

        if (!categoryExists)
        {
            throw new NotFoundException(nameof(Category), request.CategoryId);
        }

        var oldSignedAmount = transaction.Type == TransactionType.Income ? transaction.Amount : -transaction.Amount;
        var newSignedAmount = request.Type == TransactionType.Income ? request.Amount : -request.Amount;
        account.Balance = account.Balance with { Amount = account.Balance.Amount - oldSignedAmount + newSignedAmount };

        transaction.CategoryId = request.CategoryId;
        transaction.Amount = request.Amount;
        transaction.Type = request.Type;
        transaction.Date = request.Date;
        transaction.Note = request.Note;

        _transactionRepository.Update(transaction);
        _accountRepository.Update(account);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
