using Application.Common.Interfaces;
using Application.Features.Budgets.Queries.GetBudgets;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using Domain.ValueObjects;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Application.IntegrationTests.Features.Budgets.Queries;

[Collection(DatabaseCollection.Name)]
public class GetBudgetsQueryHandlerTests : IAsyncLifetime
{
    private readonly PostgresContainerFixture _fixture;
    private readonly IMapper _mapper;

    public GetBudgetsQueryHandlerTests(PostgresContainerFixture fixture)
    {
        _fixture = fixture;

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAutoMapper(cfg => cfg.AddMaps(typeof(global::Application.DependencyInjection).Assembly));
        _mapper = services.BuildServiceProvider().GetRequiredService<IMapper>();
    }

    public async Task InitializeAsync()
    {
        await using var context = _fixture.CreateContext();
        context.Budgets.RemoveRange(context.Budgets);
        context.Transactions.RemoveRange(context.Transactions);
        context.Categories.RemoveRange(context.Categories);
        context.Accounts.RemoveRange(context.Accounts);
        await context.SaveChangesAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Handle_CalculatesActualSpentAndPercentageFromExpenseTransactions()
    {
        var account = new Account { Id = Guid.NewGuid(), UserId = "user-1", Name = "Checking", Type = AccountType.Checking, Balance = new Money(0m, "USD") };
        var groceries = new Category { Id = Guid.NewGuid(), UserId = "user-1", Name = "Groceries" };
        var budget = new Budget { Id = Guid.NewGuid(), UserId = "user-1", CategoryId = groceries.Id, Month = new DateOnly(2026, 2, 1), Amount = 200m };

        var inMonthExpense1 = new Transaction { Id = Guid.NewGuid(), AccountId = account.Id, CategoryId = groceries.Id, Amount = 60m, Type = TransactionType.Expense, Date = new DateOnly(2026, 2, 5) };
        var inMonthExpense2 = new Transaction { Id = Guid.NewGuid(), AccountId = account.Id, CategoryId = groceries.Id, Amount = 40m, Type = TransactionType.Expense, Date = new DateOnly(2026, 2, 20) };
        var outOfMonthExpense = new Transaction { Id = Guid.NewGuid(), AccountId = account.Id, CategoryId = groceries.Id, Amount = 999m, Type = TransactionType.Expense, Date = new DateOnly(2026, 3, 1) };
        var inMonthIncome = new Transaction { Id = Guid.NewGuid(), AccountId = account.Id, CategoryId = groceries.Id, Amount = 500m, Type = TransactionType.Income, Date = new DateOnly(2026, 2, 10) };

        await using (var seedContext = _fixture.CreateContext())
        {
            seedContext.Accounts.Add(account);
            seedContext.Categories.Add(groceries);
            seedContext.Budgets.Add(budget);
            seedContext.Transactions.AddRange(inMonthExpense1, inMonthExpense2, outOfMonthExpense, inMonthIncome);
            await seedContext.SaveChangesAsync();
        }

        await using var queryContext = _fixture.CreateContext();
        IApplicationDbContext appContext = queryContext;
        var handler = new GetBudgetsQueryHandler(appContext, new TestCurrentUserService("user-1"), _mapper);

        var result = await handler.Handle(new GetBudgetsQuery(2026, 2), CancellationToken.None);

        var dto = Assert.Single(result);
        Assert.Equal("Groceries", dto.CategoryName);
        Assert.Equal(200m, dto.BudgetedAmount);
        Assert.Equal(100m, dto.ActualSpent);
        Assert.Equal(50m, dto.PercentageUsed);
    }

    [Fact]
    public async Task Handle_WithNoBudgetsForMonth_ReturnsEmptyList()
    {
        await using var queryContext = _fixture.CreateContext();
        IApplicationDbContext appContext = queryContext;
        var handler = new GetBudgetsQueryHandler(appContext, new TestCurrentUserService("user-1"), _mapper);

        var result = await handler.Handle(new GetBudgetsQuery(2026, 6), CancellationToken.None);

        Assert.Empty(result);
    }
}
