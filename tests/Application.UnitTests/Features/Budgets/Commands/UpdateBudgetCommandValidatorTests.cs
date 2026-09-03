using Application.Features.Budgets.Commands.UpdateBudget;
using Xunit;

namespace Application.UnitTests.Features.Budgets.Commands;

public class UpdateBudgetCommandValidatorTests
{
    private readonly UpdateBudgetCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_HasNoErrors()
    {
        var result = _validator.Validate(new UpdateBudgetCommand { Id = Guid.NewGuid(), Amount = 400m });

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WithEmptyId_HasError()
    {
        var result = _validator.Validate(new UpdateBudgetCommand { Id = Guid.Empty, Amount = 400m });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateBudgetCommand.Id));
    }

    [Fact]
    public void Validate_WithNonPositiveAmount_HasError()
    {
        var result = _validator.Validate(new UpdateBudgetCommand { Id = Guid.NewGuid(), Amount = 0m });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateBudgetCommand.Amount));
    }

    [Fact]
    public void Validate_WithAmountTooLarge_HasError()
    {
        var result = _validator.Validate(new UpdateBudgetCommand { Id = Guid.NewGuid(), Amount = 1_000_000_000_000m });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateBudgetCommand.Amount));
    }

    [Fact]
    public void Validate_WithMoreThanTwoDecimalPlaces_HasError()
    {
        var result = _validator.Validate(new UpdateBudgetCommand { Id = Guid.NewGuid(), Amount = 10.001m });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateBudgetCommand.Amount));
    }
}
