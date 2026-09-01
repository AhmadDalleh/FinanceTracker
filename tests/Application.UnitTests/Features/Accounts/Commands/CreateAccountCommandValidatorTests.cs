using Application.Features.Accounts.Commands.CreateAccount;
using Domain.Enums;
using Xunit;

namespace Application.UnitTests.Features.Accounts.Commands;

public class CreateAccountCommandValidatorTests
{
    private readonly CreateAccountCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_HasNoErrors()
    {
        var command = new CreateAccountCommand
        {
            Name = "Checking",
            Type = AccountType.Checking,
            StartingBalance = 100m,
            Currency = "USD"
        };

        var result = _validator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WithEmptyName_HasError()
    {
        var command = new CreateAccountCommand
        {
            Name = string.Empty,
            Type = AccountType.Checking,
            StartingBalance = 100m,
            Currency = "USD"
        };

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateAccountCommand.Name));
    }

    [Fact]
    public void Validate_WithNegativeStartingBalance_HasError()
    {
        var command = new CreateAccountCommand
        {
            Name = "Checking",
            Type = AccountType.Checking,
            StartingBalance = -1m,
            Currency = "USD"
        };

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateAccountCommand.StartingBalance));
    }

    [Theory]
    [InlineData("")]
    [InlineData("US")]
    [InlineData("USDD")]
    public void Validate_WithInvalidCurrency_HasError(string currency)
    {
        var command = new CreateAccountCommand
        {
            Name = "Checking",
            Type = AccountType.Checking,
            StartingBalance = 100m,
            Currency = currency
        };

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateAccountCommand.Currency));
    }
}
