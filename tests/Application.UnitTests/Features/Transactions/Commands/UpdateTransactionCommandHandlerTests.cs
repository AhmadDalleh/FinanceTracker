using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Features.Transactions.Commands.UpdateTransaction;
using Domain.Entities;
using Domain.Enums;
using Domain.ValueObjects;
using Moq;
using Xunit;

namespace Application.UnitTests.Features.Transactions.Commands;

public class UpdateTransactionCommandHandlerTests
{
    private readonly Mock<ITransactionRepository> _transactionRepository = new();
    private readonly Mock<IAccountRepository> _accountRepository = new();
    private readonly Mock<ICategoryRepository> _categoryRepository = new();
    private readonly Mock<IApplicationDbContext> _context = new();
    private readonly Mock<ICurrentUserService> _currentUserService = new();
    private readonly UpdateTransactionCommandHandler _handler;

    public UpdateTransactionCommandHandlerTests()
    {
        _handler = new UpdateTransactionCommandHandler(
            _transactionRepository.Object,
            _accountRepository.Object,
            _categoryRepository.Object,
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

    private static Transaction CreateTransaction(Guid accountId, decimal amount, TransactionType type) => new()
    {
        Id = Guid.NewGuid(),
        AccountId = accountId,
        CategoryId = Guid.NewGuid(),
        Amount = amount,
        Type = type,
        Date = new DateOnly(2026, 1, 10)
    };

    [Fact]
    public async Task Handle_WhenChangingExpenseAmount_RecalculatesBalanceByDelta()
    {
        // Account has 100 after a 20 expense was already applied (started at 120).
        var account = CreateAccount("user-1", 100m);
        var transaction = CreateTransaction(account.Id, 20m, TransactionType.Expense);

        _currentUserService.Setup(s => s.UserId).Returns("user-1");
        _transactionRepository.Setup(r => r.GetByIdAsync(transaction.Id, It.IsAny<CancellationToken>())).ReturnsAsync(transaction);
        _accountRepository.Setup(r => r.GetByIdAsync(account.Id, It.IsAny<CancellationToken>())).ReturnsAsync(account);
        _categoryRepository.Setup(r => r.ExistsAsync(It.IsAny<Guid>(), "user-1", It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var command = new UpdateTransactionCommand
        {
            Id = transaction.Id,
            CategoryId = Guid.NewGuid(),
            Amount = 35m,
            Type = TransactionType.Expense,
            Date = new DateOnly(2026, 1, 12),
            Note = "Corrected"
        };

        await _handler.Handle(command, CancellationToken.None);

        // Reverse the old -20, apply the new -35: 100 + 20 - 35 = 85
        Assert.Equal(85m, account.Balance.Amount);
        Assert.Equal(35m, transaction.Amount);
        Assert.Equal("Corrected", transaction.Note);
        _context.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenChangingTypeFromExpenseToIncome_AppliesCorrectDelta()
    {
        var account = CreateAccount("user-1", 100m);
        var transaction = CreateTransaction(account.Id, 20m, TransactionType.Expense);

        _currentUserService.Setup(s => s.UserId).Returns("user-1");
        _transactionRepository.Setup(r => r.GetByIdAsync(transaction.Id, It.IsAny<CancellationToken>())).ReturnsAsync(transaction);
        _accountRepository.Setup(r => r.GetByIdAsync(account.Id, It.IsAny<CancellationToken>())).ReturnsAsync(account);
        _categoryRepository.Setup(r => r.ExistsAsync(It.IsAny<Guid>(), "user-1", It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var command = new UpdateTransactionCommand
        {
            Id = transaction.Id,
            CategoryId = Guid.NewGuid(),
            Amount = 20m,
            Type = TransactionType.Income,
            Date = new DateOnly(2026, 1, 12)
        };

        await _handler.Handle(command, CancellationToken.None);

        // Reverse the old -20, apply the new +20: 100 + 20 + 20 = 140
        Assert.Equal(140m, account.Balance.Amount);
    }

    [Fact]
    public async Task Handle_WhenTransactionDoesNotExist_ThrowsNotFoundException()
    {
        _currentUserService.Setup(s => s.UserId).Returns("user-1");
        _transactionRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Transaction?)null);

        var command = new UpdateTransactionCommand { Id = Guid.NewGuid(), CategoryId = Guid.NewGuid(), Amount = 10m, Type = TransactionType.Expense, Date = new DateOnly(2026, 1, 1) };

        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenAccountOwnedByAnotherUser_ThrowsForbiddenAccessException()
    {
        var account = CreateAccount("someone-else", 100m);
        var transaction = CreateTransaction(account.Id, 20m, TransactionType.Expense);

        _currentUserService.Setup(s => s.UserId).Returns("user-1");
        _transactionRepository.Setup(r => r.GetByIdAsync(transaction.Id, It.IsAny<CancellationToken>())).ReturnsAsync(transaction);
        _accountRepository.Setup(r => r.GetByIdAsync(account.Id, It.IsAny<CancellationToken>())).ReturnsAsync(account);

        var command = new UpdateTransactionCommand { Id = transaction.Id, CategoryId = Guid.NewGuid(), Amount = 10m, Type = TransactionType.Expense, Date = new DateOnly(2026, 1, 1) };

        await Assert.ThrowsAsync<ForbiddenAccessException>(() => _handler.Handle(command, CancellationToken.None));
    }
}
