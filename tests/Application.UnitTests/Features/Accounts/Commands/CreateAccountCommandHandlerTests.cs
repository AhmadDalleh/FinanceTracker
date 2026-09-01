using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Features.Accounts.Commands.CreateAccount;
using Domain.Entities;
using Domain.Enums;
using Moq;
using Xunit;

namespace Application.UnitTests.Features.Accounts.Commands;

public class CreateAccountCommandHandlerTests
{
    private readonly Mock<IAccountRepository> _accountRepository = new();
    private readonly Mock<IApplicationDbContext> _context = new();
    private readonly Mock<ICurrentUserService> _currentUserService = new();
    private readonly CreateAccountCommandHandler _handler;

    public CreateAccountCommandHandlerTests()
    {
        _handler = new CreateAccountCommandHandler(_accountRepository.Object, _context.Object, _currentUserService.Object);
    }

    [Fact]
    public async Task Handle_WithAuthenticatedUser_AddsAccountAndSaves()
    {
        _currentUserService.Setup(s => s.UserId).Returns("user-1");
        var command = new CreateAccountCommand
        {
            Name = "Checking",
            Type = AccountType.Checking,
            StartingBalance = 250m,
            Currency = "USD"
        };

        var id = await _handler.Handle(command, CancellationToken.None);

        Assert.NotEqual(Guid.Empty, id);
        _accountRepository.Verify(r => r.AddAsync(
            It.Is<Account>(a =>
                a.Id == id &&
                a.UserId == "user-1" &&
                a.Name == "Checking" &&
                a.Type == AccountType.Checking &&
                a.Balance.Amount == 250m &&
                a.Balance.Currency == "USD"),
            It.IsAny<CancellationToken>()), Times.Once);
        _context.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithoutAuthenticatedUser_ThrowsForbiddenAccessException()
    {
        _currentUserService.Setup(s => s.UserId).Returns((string?)null);
        var command = new CreateAccountCommand
        {
            Name = "Checking",
            Type = AccountType.Checking,
            StartingBalance = 0m,
            Currency = "USD"
        };

        await Assert.ThrowsAsync<ForbiddenAccessException>(() => _handler.Handle(command, CancellationToken.None));
    }
}
