using System.Net;
using System.Net.Http.Json;
using Application.Features.Auth;
using Application.Features.Auth.Commands.ForgotPassword;
using Application.Features.Auth.Commands.Login;
using Application.Features.Auth.Commands.Logout;
using Application.Features.Auth.Commands.RefreshToken;
using Application.Features.Auth.Commands.Register;
using Application.Features.Auth.Commands.ResetPassword;
using Xunit;

namespace Api.FunctionalTests.Controllers;

public class AuthControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public AuthControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        factory.EnsureDatabaseCreated();
        factory.EmailSender.Reset();
    }

    [Fact]
    public async Task Register_WithNewEmail_ReturnsTokenAndUserInfo()
    {
        var response = await _client.PostAsJsonAsync("/api/Auth/register", new RegisterCommand
        {
            Email = $"{Guid.NewGuid()}@example.com",
            Password = "Password1"
        });

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<AuthResultDto>();

        Assert.NotNull(result);
        Assert.False(string.IsNullOrWhiteSpace(result!.Token));
        Assert.True(result.ExpiresAt > DateTimeOffset.UtcNow);
    }

    [Fact]
    public async Task Register_WithWeakPassword_ReturnsBadRequest()
    {
        var response = await _client.PostAsJsonAsync("/api/Auth/register", new RegisterCommand
        {
            Email = $"{Guid.NewGuid()}@example.com",
            Password = "short"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Register_WithAlreadyRegisteredEmail_ReturnsBadRequest()
    {
        var email = $"{Guid.NewGuid()}@example.com";
        var command = new RegisterCommand { Email = email, Password = "Password1" };

        await _client.PostAsJsonAsync("/api/Auth/register", command);
        var response = await _client.PostAsJsonAsync("/api/Auth/register", command);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithCorrectCredentials_ReturnsToken()
    {
        var email = $"{Guid.NewGuid()}@example.com";
        await _client.PostAsJsonAsync("/api/Auth/register", new RegisterCommand { Email = email, Password = "Password1" });

        var response = await _client.PostAsJsonAsync("/api/Auth/login", new LoginCommand { Email = email, Password = "Password1" });
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<AuthResultDto>();
        Assert.NotNull(result);
        Assert.Equal(email, result!.Email);
        Assert.False(string.IsNullOrWhiteSpace(result.RefreshToken));
    }

    [Fact]
    public async Task Login_WithWrongPassword_ReturnsUnauthorized()
    {
        var email = $"{Guid.NewGuid()}@example.com";
        await _client.PostAsJsonAsync("/api/Auth/register", new RegisterCommand { Email = email, Password = "Password1" });

        var response = await _client.PostAsJsonAsync("/api/Auth/login", new LoginCommand { Email = email, Password = "WrongPassword1" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithUnknownEmail_ReturnsUnauthorized()
    {
        var response = await _client.PostAsJsonAsync("/api/Auth/login", new LoginCommand
        {
            Email = $"{Guid.NewGuid()}@example.com",
            Password = "Password1"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ForgotPassword_WithRegisteredEmail_SendsResetToken()
    {
        var email = $"{Guid.NewGuid()}@example.com";
        await _client.PostAsJsonAsync("/api/Auth/register", new RegisterCommand { Email = email, Password = "Password1" });

        var response = await _client.PostAsJsonAsync("/api/Auth/forgot-password", new ForgotPasswordCommand { Email = email });

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(email, _factory.EmailSender.LastEmail);
        Assert.False(string.IsNullOrWhiteSpace(_factory.EmailSender.LastResetToken));
    }

    [Fact]
    public async Task ForgotPassword_WithUnknownEmail_ReturnsNoContentWithoutSendingEmail()
    {
        var response = await _client.PostAsJsonAsync("/api/Auth/forgot-password", new ForgotPasswordCommand
        {
            Email = $"{Guid.NewGuid()}@example.com"
        });

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Null(_factory.EmailSender.LastResetToken);
    }

    [Fact]
    public async Task ResetPassword_WithValidToken_AllowsLoginWithNewPassword()
    {
        var email = $"{Guid.NewGuid()}@example.com";
        await _client.PostAsJsonAsync("/api/Auth/register", new RegisterCommand { Email = email, Password = "Password1" });
        await _client.PostAsJsonAsync("/api/Auth/forgot-password", new ForgotPasswordCommand { Email = email });
        var token = _factory.EmailSender.LastResetToken!;

        var resetResponse = await _client.PostAsJsonAsync("/api/Auth/reset-password", new ResetPasswordCommand
        {
            Email = email,
            Token = token,
            NewPassword = "NewPassword1"
        });
        Assert.Equal(HttpStatusCode.NoContent, resetResponse.StatusCode);

        var oldPasswordLogin = await _client.PostAsJsonAsync("/api/Auth/login", new LoginCommand { Email = email, Password = "Password1" });
        Assert.Equal(HttpStatusCode.Unauthorized, oldPasswordLogin.StatusCode);

        var newPasswordLogin = await _client.PostAsJsonAsync("/api/Auth/login", new LoginCommand { Email = email, Password = "NewPassword1" });
        Assert.Equal(HttpStatusCode.OK, newPasswordLogin.StatusCode);
    }

    [Fact]
    public async Task ResetPassword_WithTokenReusedAfterSuccess_ReturnsBadRequest()
    {
        var email = $"{Guid.NewGuid()}@example.com";
        await _client.PostAsJsonAsync("/api/Auth/register", new RegisterCommand { Email = email, Password = "Password1" });
        await _client.PostAsJsonAsync("/api/Auth/forgot-password", new ForgotPasswordCommand { Email = email });
        var token = _factory.EmailSender.LastResetToken!;
        var command = new ResetPasswordCommand { Email = email, Token = token, NewPassword = "NewPassword1" };

        await _client.PostAsJsonAsync("/api/Auth/reset-password", command);
        var secondAttempt = await _client.PostAsJsonAsync("/api/Auth/reset-password", command);

        Assert.Equal(HttpStatusCode.BadRequest, secondAttempt.StatusCode);
    }

    [Fact]
    public async Task ResetPassword_WithInvalidToken_ReturnsBadRequest()
    {
        var email = $"{Guid.NewGuid()}@example.com";
        await _client.PostAsJsonAsync("/api/Auth/register", new RegisterCommand { Email = email, Password = "Password1" });

        var response = await _client.PostAsJsonAsync("/api/Auth/reset-password", new ResetPasswordCommand
        {
            Email = email,
            Token = "not-a-real-token",
            NewPassword = "NewPassword1"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Refresh_WithValidToken_ReturnsNewTokenPair()
    {
        var email = $"{Guid.NewGuid()}@example.com";
        var registerResponse = await _client.PostAsJsonAsync("/api/Auth/register", new RegisterCommand { Email = email, Password = "Password1" });
        var registerResult = await registerResponse.Content.ReadFromJsonAsync<AuthResultDto>();

        var response = await _client.PostAsJsonAsync("/api/Auth/refresh", new RefreshTokenCommand
        {
            RefreshToken = registerResult!.RefreshToken
        });
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<AuthResultDto>();
        Assert.NotNull(result);
        Assert.False(string.IsNullOrWhiteSpace(result!.Token));
        Assert.NotEqual(registerResult.Token, result.Token);
        Assert.NotEqual(registerResult.RefreshToken, result.RefreshToken);
    }

    [Fact]
    public async Task Refresh_WithAlreadyUsedToken_ReturnsBadRequest()
    {
        var email = $"{Guid.NewGuid()}@example.com";
        var registerResponse = await _client.PostAsJsonAsync("/api/Auth/register", new RegisterCommand { Email = email, Password = "Password1" });
        var registerResult = await registerResponse.Content.ReadFromJsonAsync<AuthResultDto>();
        var command = new RefreshTokenCommand { RefreshToken = registerResult!.RefreshToken };

        await _client.PostAsJsonAsync("/api/Auth/refresh", command);
        var secondAttempt = await _client.PostAsJsonAsync("/api/Auth/refresh", command);

        Assert.Equal(HttpStatusCode.BadRequest, secondAttempt.StatusCode);
    }

    [Fact]
    public async Task Refresh_WithInvalidToken_ReturnsBadRequest()
    {
        var response = await _client.PostAsJsonAsync("/api/Auth/refresh", new RefreshTokenCommand
        {
            RefreshToken = "not-a-real-token"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Logout_WithValidToken_RevokesItSoItCanNoLongerBeRefreshed()
    {
        var email = $"{Guid.NewGuid()}@example.com";
        var registerResponse = await _client.PostAsJsonAsync("/api/Auth/register", new RegisterCommand { Email = email, Password = "Password1" });
        var registerResult = await registerResponse.Content.ReadFromJsonAsync<AuthResultDto>();

        var logoutResponse = await _client.PostAsJsonAsync("/api/Auth/logout", new LogoutCommand { RefreshToken = registerResult!.RefreshToken });
        Assert.Equal(HttpStatusCode.NoContent, logoutResponse.StatusCode);

        var refreshAfterLogout = await _client.PostAsJsonAsync("/api/Auth/refresh", new RefreshTokenCommand { RefreshToken = registerResult.RefreshToken });
        Assert.Equal(HttpStatusCode.BadRequest, refreshAfterLogout.StatusCode);
    }

    [Fact]
    public async Task Logout_WithUnknownToken_ReturnsNoContent()
    {
        var response = await _client.PostAsJsonAsync("/api/Auth/logout", new LogoutCommand { RefreshToken = "not-a-real-token" });

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }
}
