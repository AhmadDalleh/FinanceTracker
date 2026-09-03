using System.Net;
using System.Net.Http.Json;
using System.Text;
using Application.Features.Accounts.Commands.CreateAccount;
using Application.Features.Transactions;
using Application.Features.Transactions.Commands.CreateTransaction;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Api.FunctionalTests.Controllers;

public class TransactionsControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public TransactionsControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        factory.EnsureDatabaseCreated();
    }

    private Guid SeedCategory(string name = "Groceries")
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var category = new Category
        {
            Id = Guid.NewGuid(),
            UserId = TestAuthHandler.TestUserId,
            Name = name
        };
        context.Categories.Add(category);
        context.SaveChanges();

        return category.Id;
    }

    private async Task<Guid> CreateAccountAsync()
    {
        var response = await _client.PostAsJsonAsync("/api/Accounts", new CreateAccountCommand
        {
            Name = "Checking",
            Type = AccountType.Checking,
            StartingBalance = 100m,
            Currency = "USD"
        });
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Guid>();
    }

    [Fact]
    public async Task Create_WithValidCommand_ReturnsCreatedAndAdjustsAccountBalance()
    {
        var accountId = await CreateAccountAsync();
        var categoryId = SeedCategory();

        var command = new CreateTransactionCommand
        {
            AccountId = accountId,
            CategoryId = categoryId,
            Amount = 30m,
            Type = TransactionType.Expense,
            Date = new DateOnly(2026, 1, 15)
        };

        var response = await _client.PostAsJsonAsync("/api/Transactions", command);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var accountResponse = await _client.GetAsync($"/api/Accounts/{accountId}");
        var account = await accountResponse.Content.ReadFromJsonAsync<Application.Features.Accounts.AccountDto>();
        Assert.Equal(70m, account!.Balance);
    }

    [Fact]
    public async Task Create_WithNonPositiveAmount_ReturnsBadRequest()
    {
        var accountId = await CreateAccountAsync();
        var categoryId = SeedCategory();

        var command = new CreateTransactionCommand
        {
            AccountId = accountId,
            CategoryId = categoryId,
            Amount = 0m,
            Type = TransactionType.Expense,
            Date = new DateOnly(2026, 1, 15)
        };

        var response = await _client.PostAsJsonAsync("/api/Transactions", command);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_WithMalformedTypeValue_ReturnsFriendlyErrorWithoutLeakingInternalDetails()
    {
        var accountId = await CreateAccountAsync();
        var categoryId = SeedCategory();

        var json = $$"""
        {
            "accountId": "{{accountId}}",
            "categoryId": "{{categoryId}}",
            "amount": 30,
            "type": "not-a-real-type",
            "date": "2026-01-15"
        }
        """;
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/Transactions", content);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("command field", body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("JsonException", body, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("check your input", body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Create_WithAmountTooLarge_ReturnsBadRequest()
    {
        var accountId = await CreateAccountAsync();
        var categoryId = SeedCategory();

        var command = new CreateTransactionCommand
        {
            AccountId = accountId,
            CategoryId = categoryId,
            Amount = 1_000_000_000_000m,
            Type = TransactionType.Expense,
            Date = new DateOnly(2026, 1, 15)
        };

        var response = await _client.PostAsJsonAsync("/api/Transactions", command);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_WithUnknownCategory_ReturnsNotFound()
    {
        var accountId = await CreateAccountAsync();

        var command = new CreateTransactionCommand
        {
            AccountId = accountId,
            CategoryId = Guid.NewGuid(),
            Amount = 30m,
            Type = TransactionType.Expense,
            Date = new DateOnly(2026, 1, 15)
        };

        var response = await _client.PostAsJsonAsync("/api/Transactions", command);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_AfterCreating_ReversesAccountBalance()
    {
        var accountId = await CreateAccountAsync();
        var categoryId = SeedCategory();

        var createResponse = await _client.PostAsJsonAsync("/api/Transactions", new CreateTransactionCommand
        {
            AccountId = accountId,
            CategoryId = categoryId,
            Amount = 30m,
            Type = TransactionType.Expense,
            Date = new DateOnly(2026, 1, 15)
        });
        var transactionId = await createResponse.Content.ReadFromJsonAsync<Guid>();

        var deleteResponse = await _client.DeleteAsync($"/api/Transactions/{transactionId}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var accountResponse = await _client.GetAsync($"/api/Accounts/{accountId}");
        var account = await accountResponse.Content.ReadFromJsonAsync<Application.Features.Accounts.AccountDto>();
        Assert.Equal(100m, account!.Balance);
    }

    [Fact]
    public async Task GetTransactions_FiltersByAccountId()
    {
        var accountId = await CreateAccountAsync();
        var categoryId = SeedCategory();

        await _client.PostAsJsonAsync("/api/Transactions", new CreateTransactionCommand
        {
            AccountId = accountId,
            CategoryId = categoryId,
            Amount = 15m,
            Type = TransactionType.Income,
            Date = new DateOnly(2026, 1, 20)
        });

        var response = await _client.GetAsync($"/api/Transactions?AccountId={accountId}");
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains(accountId.ToString(), body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetById_AfterCreating_ReturnsIt()
    {
        var accountId = await CreateAccountAsync();
        var categoryId = SeedCategory();

        var createResponse = await _client.PostAsJsonAsync("/api/Transactions", new CreateTransactionCommand
        {
            AccountId = accountId,
            CategoryId = categoryId,
            Amount = 42m,
            Type = TransactionType.Expense,
            Date = new DateOnly(2026, 1, 25),
            Note = "Lookup me"
        });
        var transactionId = await createResponse.Content.ReadFromJsonAsync<Guid>();

        var response = await _client.GetAsync($"/api/Transactions/{transactionId}");
        response.EnsureSuccessStatusCode();

        var transaction = await response.Content.ReadFromJsonAsync<TransactionDto>();
        Assert.NotNull(transaction);
        Assert.Equal(transactionId, transaction!.Id);
        Assert.Equal(42m, transaction.Amount);
        Assert.Equal("Lookup me", transaction.Note);
    }

    [Fact]
    public async Task GetById_WithUnknownId_ReturnsNotFound()
    {
        var response = await _client.GetAsync($"/api/Transactions/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
