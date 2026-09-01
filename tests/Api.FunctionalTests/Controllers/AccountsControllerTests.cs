using System.Net;
using System.Net.Http.Json;
using Application.Features.Accounts;
using Application.Features.Accounts.Commands.CreateAccount;
using Domain.Enums;
using Xunit;

namespace Api.FunctionalTests.Controllers;

public class AccountsControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AccountsControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
        factory.EnsureDatabaseCreated();
    }

    [Fact]
    public async Task Create_WithValidCommand_ReturnsCreated()
    {
        var command = new CreateAccountCommand
        {
            Name = "Checking",
            Type = AccountType.Checking,
            StartingBalance = 100m,
            Currency = "USD"
        };

        var response = await _client.PostAsJsonAsync("/api/Accounts", command);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Create_WithEmptyName_ReturnsBadRequest()
    {
        var command = new CreateAccountCommand
        {
            Name = string.Empty,
            Type = AccountType.Checking,
            StartingBalance = 100m,
            Currency = "USD"
        };

        var response = await _client.PostAsJsonAsync("/api/Accounts", command);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetAccounts_AfterCreatingAnAccount_ReturnsIt()
    {
        var command = new CreateAccountCommand
        {
            Name = "Savings",
            Type = AccountType.Savings,
            StartingBalance = 500m,
            Currency = "USD"
        };
        await _client.PostAsJsonAsync("/api/Accounts", command);

        var response = await _client.GetAsync("/api/Accounts");
        response.EnsureSuccessStatusCode();

        var accounts = await response.Content.ReadFromJsonAsync<List<AccountDto>>();

        Assert.NotNull(accounts);
        Assert.Contains(accounts, a => a.Name == "Savings" && a.Balance == 500m);
    }

    [Fact]
    public async Task GetById_WithUnknownId_ReturnsNotFound()
    {
        var response = await _client.GetAsync($"/api/Accounts/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
