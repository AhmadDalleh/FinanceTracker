using Application.Features.Auth.Commands.RefreshToken;
using Xunit;

namespace Application.UnitTests.Features.Auth.Commands;

public class RefreshTokenCommandValidatorTests
{
    private readonly RefreshTokenCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_HasNoErrors()
    {
        var result = _validator.Validate(new RefreshTokenCommand { RefreshToken = "some-token" });

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WithEmptyToken_HasError()
    {
        var result = _validator.Validate(new RefreshTokenCommand { RefreshToken = "" });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(RefreshTokenCommand.RefreshToken));
    }
}
