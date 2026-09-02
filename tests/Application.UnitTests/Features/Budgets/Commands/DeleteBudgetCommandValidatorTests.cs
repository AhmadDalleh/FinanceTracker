using Application.Features.Budgets.Commands.DeleteBudget;
using Xunit;

namespace Application.UnitTests.Features.Budgets.Commands;

public class DeleteBudgetCommandValidatorTests
{
    private readonly DeleteBudgetCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidId_HasNoErrors()
    {
        var result = _validator.Validate(new DeleteBudgetCommand(Guid.NewGuid()));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WithEmptyId_HasError()
    {
        var result = _validator.Validate(new DeleteBudgetCommand(Guid.Empty));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(DeleteBudgetCommand.Id));
    }
}
