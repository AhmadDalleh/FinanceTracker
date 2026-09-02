using Application.Features.Auth.Commands.Register;
using Xunit;

namespace Application.UnitTests.Features.Auth.Commands;

public class RegisterCommandValidatorTests
{
    private readonly RegisterCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_HasNoErrors()
    {
        var result = _validator.Validate(new RegisterCommand { Email = "user@example.com", Password = "Password1" });

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    public void Validate_WithInvalidEmail_HasError(string email)
    {
        var result = _validator.Validate(new RegisterCommand { Email = email, Password = "Password1" });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(RegisterCommand.Email));
    }

    [Theory]
    [InlineData("short1")]
    [InlineData("alllettersnodigit")]
    [InlineData("12345678")]
    public void Validate_WithWeakPassword_HasError(string password)
    {
        var result = _validator.Validate(new RegisterCommand { Email = "user@example.com", Password = password });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(RegisterCommand.Password));
    }
}
