using Application.Features.Auth.Commands.Logout;
using Xunit;

namespace Application.UnitTests.Features.Auth.Commands;

public class LogoutCommandValidatorTests
{
    private readonly LogoutCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_HasNoErrors()
    {
        var result = _validator.Validate(new LogoutCommand { RefreshToken = "some-token" });

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WithEmptyToken_HasError()
    {
        var result = _validator.Validate(new LogoutCommand { RefreshToken = "" });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(LogoutCommand.RefreshToken));
    }
}
