using Application.Features.Budgets.Commands.CreateBudget;
using Xunit;

namespace Application.UnitTests.Features.Budgets.Commands;

public class CreateBudgetCommandValidatorTests
{
    private readonly CreateBudgetCommandValidator _validator = new();

    private static CreateBudgetCommand ValidCommand() => new()
    {
        CategoryId = Guid.NewGuid(),
        Year = 2026,
        Month = 1,
        Amount = 300m
    };

    [Fact]
    public void Validate_WithValidCommand_HasNoErrors()
    {
        var result = _validator.Validate(ValidCommand());

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WithEmptyCategoryId_HasError()
    {
        var command = ValidCommand() with { CategoryId = Guid.Empty };

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateBudgetCommand.CategoryId));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(13)]
    public void Validate_WithInvalidMonth_HasError(int month)
    {
        var command = ValidCommand() with { Month = month };

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateBudgetCommand.Month));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void Validate_WithNonPositiveAmount_HasError(decimal amount)
    {
        var command = ValidCommand() with { Amount = amount };

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateBudgetCommand.Amount));
    }

    [Fact]
    public void Validate_WithAmountTooLarge_HasError()
    {
        var command = ValidCommand() with { Amount = 1_000_000_000_000m };

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateBudgetCommand.Amount));
    }

    [Fact]
    public void Validate_WithMoreThanTwoDecimalPlaces_HasError()
    {
        var command = ValidCommand() with { Amount = 10.001m };

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateBudgetCommand.Amount));
    }
}
