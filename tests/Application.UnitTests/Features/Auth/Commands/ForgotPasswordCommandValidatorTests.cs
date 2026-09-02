using Application.Features.Auth.Commands.ForgotPassword;
using Xunit;

namespace Application.UnitTests.Features.Auth.Commands;

public class ForgotPasswordCommandValidatorTests
{
    private readonly ForgotPasswordCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_HasNoErrors()
    {
        var result = _validator.Validate(new ForgotPasswordCommand { Email = "user@example.com" });

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    public void Validate_WithInvalidEmail_HasError(string email)
    {
        var result = _validator.Validate(new ForgotPasswordCommand { Email = email });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(ForgotPasswordCommand.Email));
    }
}
