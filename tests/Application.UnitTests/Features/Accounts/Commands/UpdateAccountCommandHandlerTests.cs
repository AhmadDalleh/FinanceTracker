using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Features.Accounts.Commands.UpdateAccount;
using Domain.Entities;
using Domain.Enums;
using Domain.ValueObjects;
using Moq;
using Xunit;

namespace Application.UnitTests.Features.Accounts.Commands;

public class UpdateAccountCommandHandlerTests
{
    private readonly Mock<IAccountRepository> _accountRepository = new();
    private readonly Mock<IApplicationDbContext> _context = new();
    private readonly Mock<ICurrentUserService> _currentUserService = new();
    private readonly UpdateAccountCommandHandler _handler;

    public UpdateAccountCommandHandlerTests()
    {
        _handler = new UpdateAccountCommandHandler(_accountRepository.Object, _context.Object, _currentUserService.Object);
    }

    private static Account CreateAccount(string userId) => new()
    {
        Id = Guid.NewGuid(),
        UserId = userId,
        Name = "Old Name",
        Type = AccountType.Checking,
        Balance = new Money(0m, "USD")
    };

    [Fact]
    public async Task Handle_WhenOwnedByCurrentUser_UpdatesAndSaves()
    {
        var account = CreateAccount("user-1");
        _currentUserService.Setup(s => s.UserId).Returns("user-1");
        _accountRepository.Setup(r => r.GetByIdAsync(account.Id, It.IsAny<CancellationToken>())).ReturnsAsync(account);

        var command = new UpdateAccountCommand { Id = account.Id, Name = "New Name", Type = AccountType.Savings };
        await _handler.Handle(command, CancellationToken.None);

        Assert.Equal("New Name", account.Name);
        Assert.Equal(AccountType.Savings, account.Type);
        _accountRepository.Verify(r => r.Update(account), Times.Once);
        _context.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenAccountDoesNotExist_ThrowsNotFoundException()
    {
        _accountRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Account?)null);

        var command = new UpdateAccountCommand { Id = Guid.NewGuid(), Name = "New Name", Type = AccountType.Savings };

        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenOwnedByAnotherUser_ThrowsForbiddenAccessException()
    {
        var account = CreateAccount("someone-else");
        _currentUserService.Setup(s => s.UserId).Returns("user-1");
        _accountRepository.Setup(r => r.GetByIdAsync(account.Id, It.IsAny<CancellationToken>())).ReturnsAsync(account);

        var command = new UpdateAccountCommand { Id = account.Id, Name = "New Name", Type = AccountType.Savings };

        await Assert.ThrowsAsync<ForbiddenAccessException>(() => _handler.Handle(command, CancellationToken.None));
    }
}
