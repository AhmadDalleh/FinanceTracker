using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Features.Budgets.Commands.UpdateBudget;
using Domain.Entities;
using Moq;
using Xunit;

namespace Application.UnitTests.Features.Budgets.Commands;

public class UpdateBudgetCommandHandlerTests
{
    private readonly Mock<IBudgetRepository> _budgetRepository = new();
    private readonly Mock<IApplicationDbContext> _context = new();
    private readonly Mock<ICurrentUserService> _currentUserService = new();
    private readonly UpdateBudgetCommandHandler _handler;

    public UpdateBudgetCommandHandlerTests()
    {
        _handler = new UpdateBudgetCommandHandler(_budgetRepository.Object, _context.Object, _currentUserService.Object);
    }

    private static Budget CreateBudget(string userId) => new()
    {
        Id = Guid.NewGuid(),
        UserId = userId,
        CategoryId = Guid.NewGuid(),
        Month = new DateOnly(2026, 1, 1),
        Amount = 300m
    };

    [Fact]
    public async Task Handle_WhenOwnedByCurrentUser_UpdatesAmountAndSaves()
    {
        var budget = CreateBudget("user-1");
        _currentUserService.Setup(s => s.UserId).Returns("user-1");
        _budgetRepository.Setup(r => r.GetByIdAsync(budget.Id, It.IsAny<CancellationToken>())).ReturnsAsync(budget);

        await _handler.Handle(new UpdateBudgetCommand { Id = budget.Id, Amount = 450m }, CancellationToken.None);

        Assert.Equal(450m, budget.Amount);
        _budgetRepository.Verify(r => r.Update(budget), Times.Once);
        _context.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenBudgetDoesNotExist_ThrowsNotFoundException()
    {
        _budgetRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Budget?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.Handle(new UpdateBudgetCommand { Id = Guid.NewGuid(), Amount = 450m }, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenOwnedByAnotherUser_ThrowsForbiddenAccessException()
    {
        var budget = CreateBudget("someone-else");
        _currentUserService.Setup(s => s.UserId).Returns("user-1");
        _budgetRepository.Setup(r => r.GetByIdAsync(budget.Id, It.IsAny<CancellationToken>())).ReturnsAsync(budget);

        await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            _handler.Handle(new UpdateBudgetCommand { Id = budget.Id, Amount = 450m }, CancellationToken.None));
    }
}
