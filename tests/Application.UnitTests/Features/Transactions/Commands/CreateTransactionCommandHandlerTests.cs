using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Features.Transactions.Commands.CreateTransaction;
using Domain.Entities;
using Domain.Enums;
using Domain.ValueObjects;
using Moq;
using Xunit;

namespace Application.UnitTests.Features.Transactions.Commands;

public class CreateTransactionCommandHandlerTests
{
    private readonly Mock<ITransactionRepository> _transactionRepository = new();
    private readonly Mock<IAccountRepository> _accountRepository = new();
    private readonly Mock<ICategoryRepository> _categoryRepository = new();
    private readonly Mock<IApplicationDbContext> _context = new();
    private readonly Mock<ICurrentUserService> _currentUserService = new();
    private readonly CreateTransactionCommandHandler _handler;

    public CreateTransactionCommandHandlerTests()
    {
        _handler = new CreateTransactionCommandHandler(
            _transactionRepository.Object,
            _accountRepository.Object,
            _categoryRepository.Object,
            _context.Object,
            _currentUserService.Object);
    }

    private static Account CreateAccount(string userId, decimal balance = 100m) => new()
    {
        Id = Guid.NewGuid(),
        UserId = userId,
        Name = "Checking",
        Type = AccountType.Checking,
        Balance = new Money(balance, "USD")
    };

    [Fact]
    public async Task Handle_WithIncome_AddsTransactionAndIncreasesBalance()
    {
        var account = CreateAccount("user-1", 100m);
        _currentUserService.Setup(s => s.UserId).Returns("user-1");
        _accountRepository.Setup(r => r.GetByIdAsync(account.Id, It.IsAny<CancellationToken>())).ReturnsAsync(account);
        _categoryRepository.Setup(r => r.ExistsAsync(It.IsAny<Guid>(), "user-1", It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var command = new CreateTransactionCommand
        {
            AccountId = account.Id,
            CategoryId = Guid.NewGuid(),
            Amount = 50m,
            Type = TransactionType.Income,
            Date = new DateOnly(2026, 1, 15)
        };

        var id = await _handler.Handle(command, CancellationToken.None);

        Assert.NotEqual(Guid.Empty, id);
        Assert.Equal(150m, account.Balance.Amount);
        _transactionRepository.Verify(r => r.AddAsync(It.Is<Transaction>(t => t.Id == id && t.Amount == 50m), It.IsAny<CancellationToken>()), Times.Once);
        _accountRepository.Verify(r => r.Update(account), Times.Once);
        _context.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithExpense_DecreasesBalance()
    {
        var account = CreateAccount("user-1", 100m);
        _currentUserService.Setup(s => s.UserId).Returns("user-1");
        _accountRepository.Setup(r => r.GetByIdAsync(account.Id, It.IsAny<CancellationToken>())).ReturnsAsync(account);
        _categoryRepository.Setup(r => r.ExistsAsync(It.IsAny<Guid>(), "user-1", It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var command = new CreateTransactionCommand
        {
            AccountId = account.Id,
            CategoryId = Guid.NewGuid(),
            Amount = 30m,
            Type = TransactionType.Expense,
            Date = new DateOnly(2026, 1, 15)
        };

        await _handler.Handle(command, CancellationToken.None);

        Assert.Equal(70m, account.Balance.Amount);
    }

    [Fact]
    public async Task Handle_WhenAccountDoesNotExist_ThrowsNotFoundException()
    {
        _accountRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Account?)null);
        _currentUserService.Setup(s => s.UserId).Returns("user-1");

        var command = new CreateTransactionCommand
        {
            AccountId = Guid.NewGuid(),
            CategoryId = Guid.NewGuid(),
            Amount = 30m,
            Type = TransactionType.Expense,
            Date = new DateOnly(2026, 1, 15)
        };

        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenAccountOwnedByAnotherUser_ThrowsForbiddenAccessException()
    {
        var account = CreateAccount("someone-else");
        _currentUserService.Setup(s => s.UserId).Returns("user-1");
        _accountRepository.Setup(r => r.GetByIdAsync(account.Id, It.IsAny<CancellationToken>())).ReturnsAsync(account);

        var command = new CreateTransactionCommand
        {
            AccountId = account.Id,
            CategoryId = Guid.NewGuid(),
            Amount = 30m,
            Type = TransactionType.Expense,
            Date = new DateOnly(2026, 1, 15)
        };

        await Assert.ThrowsAsync<ForbiddenAccessException>(() => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenCategoryDoesNotExist_ThrowsNotFoundException()
    {
        var account = CreateAccount("user-1");
        _currentUserService.Setup(s => s.UserId).Returns("user-1");
        _accountRepository.Setup(r => r.GetByIdAsync(account.Id, It.IsAny<CancellationToken>())).ReturnsAsync(account);
        _categoryRepository.Setup(r => r.ExistsAsync(It.IsAny<Guid>(), "user-1", It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var command = new CreateTransactionCommand
        {
            AccountId = account.Id,
            CategoryId = Guid.NewGuid(),
            Amount = 30m,
            Type = TransactionType.Expense,
            Date = new DateOnly(2026, 1, 15)
        };

        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));
    }
}
