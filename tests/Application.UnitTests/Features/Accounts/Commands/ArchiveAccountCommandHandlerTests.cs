using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Features.Accounts.Commands.ArchiveAccount;
using Domain.Entities;
using Domain.Enums;
using Domain.ValueObjects;
using Moq;
using Xunit;

namespace Application.UnitTests.Features.Accounts.Commands;

public class ArchiveAccountCommandHandlerTests
{
    private readonly Mock<IAccountRepository> _accountRepository = new();
    private readonly Mock<IApplicationDbContext> _context = new();
    private readonly Mock<ICurrentUserService> _currentUserService = new();
    private readonly ArchiveAccountCommandHandler _handler;

    public ArchiveAccountCommandHandlerTests()
    {
        _handler = new ArchiveAccountCommandHandler(_accountRepository.Object, _context.Object, _currentUserService.Object);
    }

    private static Account CreateAccount(string userId) => new()
    {
        Id = Guid.NewGuid(),
        UserId = userId,
        Name = "Checking",
        Type = AccountType.Checking,
        Balance = new Money(0m, "USD")
    };

    [Fact]
    public async Task Handle_WhenOwnedByCurrentUser_ArchivesAndSaves()
    {
        var account = CreateAccount("user-1");
        _currentUserService.Setup(s => s.UserId).Returns("user-1");
        _accountRepository.Setup(r => r.GetByIdAsync(account.Id, It.IsAny<CancellationToken>())).ReturnsAsync(account);

        await _handler.Handle(new ArchiveAccountCommand(account.Id), CancellationToken.None);

        Assert.True(account.IsArchived);
        _accountRepository.Verify(r => r.Update(account), Times.Once);
        _context.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenAccountDoesNotExist_ThrowsNotFoundException()
    {
        _accountRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Account?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(new ArchiveAccountCommand(Guid.NewGuid()), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenOwnedByAnotherUser_ThrowsForbiddenAccessException()
    {
        var account = CreateAccount("someone-else");
        _currentUserService.Setup(s => s.UserId).Returns("user-1");
        _accountRepository.Setup(r => r.GetByIdAsync(account.Id, It.IsAny<CancellationToken>())).ReturnsAsync(account);

        await Assert.ThrowsAsync<ForbiddenAccessException>(() => _handler.Handle(new ArchiveAccountCommand(account.Id), CancellationToken.None));
    }
}
