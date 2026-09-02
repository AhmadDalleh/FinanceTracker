using System.Net;
using System.Net.Http.Json;
using Application.Features.Budgets;
using Application.Features.Budgets.Commands.CreateBudget;
using Application.Features.Budgets.Commands.UpdateBudget;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Api.FunctionalTests.Controllers;

public class BudgetsControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public BudgetsControllerTests(CustomWebApplicationFactory factory)
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

    [Fact]
    public async Task Create_WithValidCommand_ReturnsCreated()
    {
        var categoryId = SeedCategory("Groceries " + Guid.NewGuid());

        var response = await _client.PostAsJsonAsync("/api/Budgets", new CreateBudgetCommand
        {
            CategoryId = categoryId,
            Year = 2026,
            Month = 5,
            Amount = 250m
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Create_DuplicateForSameCategoryAndMonth_ReturnsBadRequest()
    {
        var categoryId = SeedCategory("Rent " + Guid.NewGuid());
        var command = new CreateBudgetCommand { CategoryId = categoryId, Year = 2026, Month = 6, Amount = 1500m };

        await _client.PostAsJsonAsync("/api/Budgets", command);
        var response = await _client.PostAsJsonAsync("/api/Budgets", command);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_WithUnknownCategory_ReturnsNotFound()
    {
        var response = await _client.PostAsJsonAsync("/api/Budgets", new CreateBudgetCommand
        {
            CategoryId = Guid.NewGuid(),
            Year = 2026,
            Month = 7,
            Amount = 100m
        });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetBudgets_ReturnsBudgetForRequestedMonth()
    {
        var categoryId = SeedCategory("Utilities " + Guid.NewGuid());
        await _client.PostAsJsonAsync("/api/Budgets", new CreateBudgetCommand { CategoryId = categoryId, Year = 2026, Month = 8, Amount = 120m });

        var response = await _client.GetAsync("/api/Budgets?year=2026&month=8");
        response.EnsureSuccessStatusCode();

        var budgets = await response.Content.ReadFromJsonAsync<List<BudgetDto>>();

        Assert.NotNull(budgets);
        Assert.Contains(budgets, b => b.CategoryId == categoryId && b.BudgetedAmount == 120m && b.ActualSpent == 0m);
    }

    [Fact]
    public async Task Update_ChangesAmount()
    {
        var categoryId = SeedCategory("Insurance " + Guid.NewGuid());
        var createResponse = await _client.PostAsJsonAsync("/api/Budgets", new CreateBudgetCommand { CategoryId = categoryId, Year = 2026, Month = 9, Amount = 80m });
        var budgetId = await createResponse.Content.ReadFromJsonAsync<Guid>();

        var updateResponse = await _client.PutAsJsonAsync($"/api/Budgets/{budgetId}", new UpdateBudgetCommand { Id = budgetId, Amount = 95m });
        Assert.Equal(HttpStatusCode.NoContent, updateResponse.StatusCode);

        var listResponse = await _client.GetAsync("/api/Budgets?year=2026&month=9");
        var budgets = await listResponse.Content.ReadFromJsonAsync<List<BudgetDto>>();
        Assert.Contains(budgets!, b => b.Id == budgetId && b.BudgetedAmount == 95m);
    }

    [Fact]
    public async Task Delete_RemovesBudgetFromList()
    {
        var categoryId = SeedCategory("Subscriptions " + Guid.NewGuid());
        var createResponse = await _client.PostAsJsonAsync("/api/Budgets", new CreateBudgetCommand { CategoryId = categoryId, Year = 2026, Month = 10, Amount = 30m });
        var budgetId = await createResponse.Content.ReadFromJsonAsync<Guid>();

        var deleteResponse = await _client.DeleteAsync($"/api/Budgets/{budgetId}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var listResponse = await _client.GetAsync("/api/Budgets?year=2026&month=10");
        var budgets = await listResponse.Content.ReadFromJsonAsync<List<BudgetDto>>();
        Assert.DoesNotContain(budgets!, b => b.Id == budgetId);
    }
}
