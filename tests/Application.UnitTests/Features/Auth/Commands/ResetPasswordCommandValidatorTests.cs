using Application.Features.Auth.Commands.ResetPassword;
using Xunit;

namespace Application.UnitTests.Features.Auth.Commands;

public class ResetPasswordCommandValidatorTests
{
    private readonly ResetPasswordCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_HasNoErrors()
    {
        var result = _validator.Validate(new ResetPasswordCommand
        {
            Email = "user@example.com",
            Token = "some-token",
            NewPassword = "Password1"
        });

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WithEmptyToken_HasError()
    {
        var result = _validator.Validate(new ResetPasswordCommand
        {
            Email = "user@example.com",
            Token = "",
            NewPassword = "Password1"
        });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(ResetPasswordCommand.Token));
    }

    [Theory]
    [InlineData("short1")]
    [InlineData("alllettersnodigit")]
    [InlineData("12345678")]
    public void Validate_WithWeakPassword_HasError(string password)
    {
        var result = _validator.Validate(new ResetPasswordCommand
        {
            Email = "user@example.com",
            Token = "some-token",
            NewPassword = password
        });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(ResetPasswordCommand.NewPassword));
    }
}
