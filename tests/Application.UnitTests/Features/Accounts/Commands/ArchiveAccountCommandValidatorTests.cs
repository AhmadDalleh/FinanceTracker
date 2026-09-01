using Application.Features.Accounts.Commands.ArchiveAccount;
using Xunit;

namespace Application.UnitTests.Features.Accounts.Commands;

public class ArchiveAccountCommandValidatorTests
{
    private readonly ArchiveAccountCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidId_HasNoErrors()
    {
        var result = _validator.Validate(new ArchiveAccountCommand(Guid.NewGuid()));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WithEmptyId_HasError()
    {
        var result = _validator.Validate(new ArchiveAccountCommand(Guid.Empty));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(ArchiveAccountCommand.Id));
    }
}
