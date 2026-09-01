using Application.Features.Transactions.Commands.CreateTransaction;
using Domain.Enums;
using Xunit;

namespace Application.UnitTests.Features.Transactions.Commands;

public class CreateTransactionCommandValidatorTests
{
    private readonly CreateTransactionCommandValidator _validator = new();

    private static CreateTransactionCommand ValidCommand() => new()
    {
        AccountId = Guid.NewGuid(),
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
    public void Validate_WithEmptyAccountId_HasError()
    {
        var command = ValidCommand() with { AccountId = Guid.Empty };

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateTransactionCommand.AccountId));
    }

    [Fact]
    public void Validate_WithEmptyCategoryId_HasError()
    {
        var command = ValidCommand() with { CategoryId = Guid.Empty };

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateTransactionCommand.CategoryId));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void Validate_WithNonPositiveAmount_HasError(decimal amount)
    {
        var command = ValidCommand() with { Amount = amount };

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateTransactionCommand.Amount));
    }

    [Fact]
    public void Validate_WithDefaultDate_HasError()
    {
        var command = ValidCommand() with { Date = default };

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateTransactionCommand.Date));
    }

    [Fact]
    public void Validate_WithTooLongNote_HasError()
    {
        var command = ValidCommand() with { Note = new string('x', 501) };

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateTransactionCommand.Note));
    }
}
