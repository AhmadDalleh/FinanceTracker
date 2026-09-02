using Application.Features.Auth.Commands.Login;
using Xunit;

namespace Application.UnitTests.Features.Auth.Commands;

public class LoginCommandValidatorTests
{
    private readonly LoginCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_HasNoErrors()
    {
        var result = _validator.Validate(new LoginCommand { Email = "user@example.com", Password = "anything" });

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WithEmptyEmail_HasError()
    {
        var result = _validator.Validate(new LoginCommand { Email = "", Password = "anything" });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(LoginCommand.Email));
    }

    [Fact]
    public void Validate_WithEmptyPassword_HasError()
    {
        var result = _validator.Validate(new LoginCommand { Email = "user@example.com", Password = "" });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(LoginCommand.Password));
    }
}
