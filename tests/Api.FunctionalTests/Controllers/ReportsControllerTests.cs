using System.Net.Http.Json;
using Application.Features.Accounts.Commands.CreateAccount;
using Application.Features.Reports;
using Application.Features.Transactions.Commands.CreateTransaction;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Api.FunctionalTests.Controllers;

public class ReportsControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public ReportsControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        factory.EnsureDatabaseCreated();
    }

    private Guid SeedCategory(string name)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var category = new Category { Id = Guid.NewGuid(), UserId = TestAuthHandler.TestUserId, Name = name };
        context.Categories.Add(category);
        context.SaveChanges();

        return category.Id;
    }

    private async Task<Guid> CreateAccountAsync()
    {
        var response = await _client.PostAsJsonAsync("/api/Accounts", new CreateAccountCommand
        {
            Name = "Report Test Account " + Guid.NewGuid(),
            Type = AccountType.Checking,
            StartingBalance = 0m,
            Currency = "USD"
        });
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Guid>();
    }

    [Fact]
    public async Task GetMonthlySummary_ReturnsIncomeExpenseAndNetPosition()
    {
        var accountId = await CreateAccountAsync();
        var categoryId = SeedCategory("Salary " + Guid.NewGuid());

        await _client.PostAsJsonAsync("/api/Transactions", new CreateTransactionCommand
        {
            AccountId = accountId,
            CategoryId = categoryId,
            Amount = 2000m,
            Type = TransactionType.Income,
            Date = new DateOnly(2026, 11, 1)
        });
        await _client.PostAsJsonAsync("/api/Transactions", new CreateTransactionCommand
        {
            AccountId = accountId,
            CategoryId = categoryId,
            Amount = 300m,
            Type = TransactionType.Expense,
            Date = new DateOnly(2026, 11, 10)
        });

        var response = await _client.GetAsync("/api/Reports/monthly-summary?year=2026&month=11");
        response.EnsureSuccessStatusCode();
        var summary = await response.Content.ReadFromJsonAsync<MonthlySummaryDto>();

        Assert.NotNull(summary);
        Assert.Equal(2000m, summary!.TotalIncome);
        Assert.Equal(300m, summary.TotalExpense);
        Assert.Equal(1700m, summary.NetPosition);
    }

    [Fact]
    public async Task GetSpendByCategory_ReturnsOnlyExpensesGroupedByCategory()
    {
        var accountId = await CreateAccountAsync();
        var categoryId = SeedCategory("Dining " + Guid.NewGuid());

        await _client.PostAsJsonAsync("/api/Transactions", new CreateTransactionCommand
        {
            AccountId = accountId,
            CategoryId = categoryId,
            Amount = 75m,
            Type = TransactionType.Expense,
            Date = new DateOnly(2026, 12, 5)
        });

        var response = await _client.GetAsync("/api/Reports/spend-by-category?year=2026&month=12");
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<List<CategorySpendDto>>();

        Assert.NotNull(result);
        Assert.Contains(result!, c => c.CategoryId == categoryId && c.TotalSpent == 75m);
    }
}
