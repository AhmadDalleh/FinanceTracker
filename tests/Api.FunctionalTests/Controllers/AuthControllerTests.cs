using System.Net;
using System.Net.Http.Json;
using Application.Features.Auth;
using Application.Features.Auth.Commands.Login;
using Application.Features.Auth.Commands.Register;
using Xunit;

namespace Api.FunctionalTests.Controllers;

public class AuthControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
        factory.EnsureDatabaseCreated();
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
}
