using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Features.Budgets.Commands.CreateBudget;
using Domain.Entities;
using Moq;
using Xunit;
using ValidationException = Application.Common.Exceptions.ValidationException;

namespace Application.UnitTests.Features.Budgets.Commands;

public class CreateBudgetCommandHandlerTests
{
    private readonly Mock<IBudgetRepository> _budgetRepository = new();
    private readonly Mock<ICategoryRepository> _categoryRepository = new();
    private readonly Mock<IApplicationDbContext> _context = new();
    private readonly Mock<ICurrentUserService> _currentUserService = new();
    private readonly CreateBudgetCommandHandler _handler;

    public CreateBudgetCommandHandlerTests()
    {
        _handler = new CreateBudgetCommandHandler(
            _budgetRepository.Object,
            _categoryRepository.Object,
            _context.Object,
            _currentUserService.Object);
    }

    private static CreateBudgetCommand ValidCommand() => new()
    {
        CategoryId = Guid.NewGuid(),
        Year = 2026,
        Month = 1,
        Amount = 300m
    };

    [Fact]
    public async Task Handle_WithValidCommand_AddsBudgetAndSaves()
    {
        _currentUserService.Setup(s => s.UserId).Returns("user-1");
        _categoryRepository.Setup(r => r.ExistsAsync(It.IsAny<Guid>(), "user-1", It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _budgetRepository.Setup(r => r.ExistsForCategoryAndMonthAsync(It.IsAny<Guid>(), It.IsAny<DateOnly>(), "user-1", It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var command = ValidCommand();
        var id = await _handler.Handle(command, CancellationToken.None);

        Assert.NotEqual(Guid.Empty, id);
        _budgetRepository.Verify(r => r.AddAsync(
            It.Is<Budget>(b => b.Id == id && b.UserId == "user-1" && b.CategoryId == command.CategoryId && b.Month == new DateOnly(2026, 1, 1) && b.Amount == 300m),
            It.IsAny<CancellationToken>()), Times.Once);
        _context.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenCategoryDoesNotExist_ThrowsNotFoundException()
    {
        _currentUserService.Setup(s => s.UserId).Returns("user-1");
        _categoryRepository.Setup(r => r.ExistsAsync(It.IsAny<Guid>(), "user-1", It.IsAny<CancellationToken>())).ReturnsAsync(false);

        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(ValidCommand(), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenBudgetAlreadyExistsForCategoryAndMonth_ThrowsValidationException()
    {
        _currentUserService.Setup(s => s.UserId).Returns("user-1");
        _categoryRepository.Setup(r => r.ExistsAsync(It.IsAny<Guid>(), "user-1", It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _budgetRepository.Setup(r => r.ExistsForCategoryAndMonthAsync(It.IsAny<Guid>(), It.IsAny<DateOnly>(), "user-1", It.IsAny<CancellationToken>())).ReturnsAsync(true);

        await Assert.ThrowsAsync<ValidationException>(() => _handler.Handle(ValidCommand(), CancellationToken.None));
    }
}
