using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Features.Transactions.Commands.DeleteTransaction;
using Domain.Entities;
using Domain.Enums;
using Domain.ValueObjects;
using Moq;
using Xunit;

namespace Application.UnitTests.Features.Transactions.Commands;

public class DeleteTransactionCommandHandlerTests
{
    private readonly Mock<ITransactionRepository> _transactionRepository = new();
    private readonly Mock<IAccountRepository> _accountRepository = new();
    private readonly Mock<IApplicationDbContext> _context = new();
    private readonly Mock<ICurrentUserService> _currentUserService = new();
    private readonly DeleteTransactionCommandHandler _handler;

    public DeleteTransactionCommandHandlerTests()
    {
        _handler = new DeleteTransactionCommandHandler(
            _transactionRepository.Object,
            _accountRepository.Object,
            _context.Object,
            _currentUserService.Object);
    }

    private static Account CreateAccount(string userId, decimal balance) => new()
    {
        Id = Guid.NewGuid(),
        UserId = userId,
        Name = "Checking",
        Type = AccountType.Checking,
        Balance = new Money(balance, "USD")
    };

    [Fact]
    public async Task Handle_WhenDeletingIncomeTransaction_ReversesBalanceAndRemoves()
    {
        var account = CreateAccount("user-1", 150m);
        var transaction = new Transaction
        {
            Id = Guid.NewGuid(),
            AccountId = account.Id,
            CategoryId = Guid.NewGuid(),
            Amount = 50m,
            Type = TransactionType.Income,
            Date = new DateOnly(2026, 1, 10)
        };

        _currentUserService.Setup(s => s.UserId).Returns("user-1");
        _transactionRepository.Setup(r => r.GetByIdAsync(transaction.Id, It.IsAny<CancellationToken>())).ReturnsAsync(transaction);
        _accountRepository.Setup(r => r.GetByIdAsync(account.Id, It.IsAny<CancellationToken>())).ReturnsAsync(account);

        await _handler.Handle(new DeleteTransactionCommand(transaction.Id), CancellationToken.None);

        Assert.Equal(100m, account.Balance.Amount);
        _transactionRepository.Verify(r => r.Remove(transaction), Times.Once);
        _accountRepository.Verify(r => r.Update(account), Times.Once);
        _context.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenTransactionDoesNotExist_ThrowsNotFoundException()
    {
        _currentUserService.Setup(s => s.UserId).Returns("user-1");
        _transactionRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Transaction?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(new DeleteTransactionCommand(Guid.NewGuid()), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenAccountOwnedByAnotherUser_ThrowsForbiddenAccessException()
    {
        var account = CreateAccount("someone-else", 100m);
        var transaction = new Transaction
        {
            Id = Guid.NewGuid(),
            AccountId = account.Id,
            CategoryId = Guid.NewGuid(),
            Amount = 50m,
            Type = TransactionType.Income,
            Date = new DateOnly(2026, 1, 10)
        };

        _currentUserService.Setup(s => s.UserId).Returns("user-1");
        _transactionRepository.Setup(r => r.GetByIdAsync(transaction.Id, It.IsAny<CancellationToken>())).ReturnsAsync(transaction);
        _accountRepository.Setup(r => r.GetByIdAsync(account.Id, It.IsAny<CancellationToken>())).ReturnsAsync(account);

        await Assert.ThrowsAsync<ForbiddenAccessException>(() => _handler.Handle(new DeleteTransactionCommand(transaction.Id), CancellationToken.None));
    }
}
