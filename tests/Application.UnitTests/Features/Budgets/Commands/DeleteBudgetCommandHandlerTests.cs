using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Features.Budgets.Commands.DeleteBudget;
using Domain.Entities;
using Moq;
using Xunit;

namespace Application.UnitTests.Features.Budgets.Commands;

public class DeleteBudgetCommandHandlerTests
{
    private readonly Mock<IBudgetRepository> _budgetRepository = new();
    private readonly Mock<IApplicationDbContext> _context = new();
    private readonly Mock<ICurrentUserService> _currentUserService = new();
    private readonly DeleteBudgetCommandHandler _handler;

    public DeleteBudgetCommandHandlerTests()
    {
        _handler = new DeleteBudgetCommandHandler(_budgetRepository.Object, _context.Object, _currentUserService.Object);
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
    public async Task Handle_WhenOwnedByCurrentUser_RemovesAndSaves()
    {
        var budget = CreateBudget("user-1");
        _currentUserService.Setup(s => s.UserId).Returns("user-1");
        _budgetRepository.Setup(r => r.GetByIdAsync(budget.Id, It.IsAny<CancellationToken>())).ReturnsAsync(budget);

        await _handler.Handle(new DeleteBudgetCommand(budget.Id), CancellationToken.None);

        _budgetRepository.Verify(r => r.Remove(budget), Times.Once);
        _context.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenBudgetDoesNotExist_ThrowsNotFoundException()
    {
        _budgetRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Budget?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(new DeleteBudgetCommand(Guid.NewGuid()), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenOwnedByAnotherUser_ThrowsForbiddenAccessException()
    {
        var budget = CreateBudget("someone-else");
        _currentUserService.Setup(s => s.UserId).Returns("user-1");
        _budgetRepository.Setup(r => r.GetByIdAsync(budget.Id, It.IsAny<CancellationToken>())).ReturnsAsync(budget);

        await Assert.ThrowsAsync<ForbiddenAccessException>(() => _handler.Handle(new DeleteBudgetCommand(budget.Id), CancellationToken.None));
    }
}
