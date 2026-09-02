using System.Net;
using System.Net.Http.Json;
using Application.Features.Accounts;
using Application.Features.Accounts.Commands.CreateAccount;
using Domain.Enums;
using Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Api.FunctionalTests.Controllers;

public class AccountsControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public AccountsControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
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

    [Fact]
    public async Task Create_SetsCreatedAtAndCreatedByViaTheAuditInterceptor()
    {
        var command = new CreateAccountCommand
        {
            Name = "Audited Account",
            Type = AccountType.Checking,
            StartingBalance = 0m,
            Currency = "USD"
        };

        var response = await _client.PostAsJsonAsync("/api/Accounts", command);
        var id = await response.Content.ReadFromJsonAsync<Guid>();

        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var account = await context.Accounts.FindAsync(id);

        Assert.NotNull(account);
        Assert.True(account!.CreatedAt > DateTimeOffset.UnixEpoch);
        Assert.Equal(TestAuthHandler.TestUserId, account.CreatedBy);
        Assert.Equal(account.CreatedAt, account.UpdatedAt);
        Assert.Equal(TestAuthHandler.TestUserId, account.UpdatedBy);
    }

    [Fact]
    public async Task Update_SetsUpdatedAtViaTheAuditInterceptor()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/Accounts", new CreateAccountCommand
        {
            Name = "Before Rename",
            Type = AccountType.Checking,
            StartingBalance = 0m,
            Currency = "USD"
        });
        var id = await createResponse.Content.ReadFromJsonAsync<Guid>();

        var updateResponse = await _client.PutAsJsonAsync($"/api/Accounts/{id}", new
        {
            Id = id,
            Name = "After Rename",
            Type = AccountType.Checking
        });
        Assert.Equal(HttpStatusCode.NoContent, updateResponse.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var account = await context.Accounts.FindAsync(id);

        Assert.NotNull(account);
        Assert.NotNull(account!.UpdatedAt);
        Assert.True(account.UpdatedAt >= account.CreatedAt);
        Assert.Equal(TestAuthHandler.TestUserId, account.UpdatedBy);
    }
}
