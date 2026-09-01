using Application.Features.Accounts.Commands.UpdateAccount;
using Domain.Enums;
using Xunit;

namespace Application.UnitTests.Features.Accounts.Commands;

public class UpdateAccountCommandValidatorTests
{
    private readonly UpdateAccountCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_HasNoErrors()
    {
        var command = new UpdateAccountCommand
        {
            Id = Guid.NewGuid(),
            Name = "Renamed",
            Type = AccountType.Savings
        };

        var result = _validator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WithEmptyId_HasError()
    {
        var command = new UpdateAccountCommand
        {
            Id = Guid.Empty,
            Name = "Renamed",
            Type = AccountType.Savings
        };

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateAccountCommand.Id));
    }

    [Fact]
    public void Validate_WithEmptyName_HasError()
    {
        var command = new UpdateAccountCommand
        {
            Id = Guid.NewGuid(),
            Name = string.Empty,
            Type = AccountType.Savings
        };

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateAccountCommand.Name));
    }
}
