using Application.Features.Transactions.Commands.UpdateTransaction;
using Domain.Enums;
using Xunit;

namespace Application.UnitTests.Features.Transactions.Commands;

public class UpdateTransactionCommandValidatorTests
{
    private readonly UpdateTransactionCommandValidator _validator = new();

    private static UpdateTransactionCommand ValidCommand() => new()
    {
        Id = Guid.NewGuid(),
        CategoryId = Guid.NewGuid(),
        Amount = 25.50m,
        Type = TransactionType.Expense,
        Date = new DateOnly(2026, 1, 15),
        Note = "Groceries"
    };

    [Fact]
    public void Validate_WithValidCommand_HasNoErrors()
    {
        var result = _validator.Validate(ValidCommand());

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WithEmptyId_HasError()
    {
        var command = ValidCommand() with { Id = Guid.Empty };

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateTransactionCommand.Id));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void Validate_WithNonPositiveAmount_HasError(decimal amount)
    {
        var command = ValidCommand() with { Amount = amount };

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateTransactionCommand.Amount));
    }

    [Fact]
    public void Validate_WithAmountTooLarge_HasError()
    {
        var command = ValidCommand() with { Amount = 1_000_000_000_000m };

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateTransactionCommand.Amount));
    }

    [Fact]
    public void Validate_WithMoreThanTwoDecimalPlaces_HasError()
    {
        var command = ValidCommand() with { Amount = 10.001m };

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateTransactionCommand.Amount));
    }
}
