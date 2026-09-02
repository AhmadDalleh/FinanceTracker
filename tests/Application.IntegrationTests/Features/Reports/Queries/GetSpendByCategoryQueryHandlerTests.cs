using Application.Common.Interfaces;
using Application.Features.Reports.Queries.GetSpendByCategory;
using Domain.Entities;
using Domain.Enums;
using Domain.ValueObjects;
using Xunit;

namespace Application.IntegrationTests.Features.Reports.Queries;

[Collection(DatabaseCollection.Name)]
public class GetSpendByCategoryQueryHandlerTests : IAsyncLifetime
{
    private readonly PostgresContainerFixture _fixture;

    public GetSpendByCategoryQueryHandlerTests(PostgresContainerFixture fixture)
    {
        _fixture = fixture;
    }

    public async Task InitializeAsync()
    {
        await using var context = _fixture.CreateContext();
        context.Transactions.RemoveRange(context.Transactions);
        context.Categories.RemoveRange(context.Categories);
        context.Accounts.RemoveRange(context.Accounts);
        await context.SaveChangesAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Handle_GroupsExpensesByCategoryOrderedByTotalDescending()
    {
        var account = new Account { Id = Guid.NewGuid(), UserId = "user-1", Name = "Checking", Type = AccountType.Checking, Balance = new Money(0m, "USD") };
        var groceries = new Category { Id = Guid.NewGuid(), UserId = "user-1", Name = "Groceries" };
        var rent = new Category { Id = Guid.NewGuid(), UserId = "user-1", Name = "Rent" };

        var groceriesExpense = new Transaction { Id = Guid.NewGuid(), AccountId = account.Id, CategoryId = groceries.Id, Amount = 150m, Type = TransactionType.Expense, Date = new DateOnly(2026, 4, 5) };
        var rentExpense = new Transaction { Id = Guid.NewGuid(), AccountId = account.Id, CategoryId = rent.Id, Amount = 1500m, Type = TransactionType.Expense, Date = new DateOnly(2026, 4, 1) };
        var income = new Transaction { Id = Guid.NewGuid(), AccountId = account.Id, CategoryId = groceries.Id, Amount = 5000m, Type = TransactionType.Income, Date = new DateOnly(2026, 4, 1) };

        await using (var seedContext = _fixture.CreateContext())
        {
            seedContext.Accounts.Add(account);
            seedContext.Categories.AddRange(groceries, rent);
            seedContext.Transactions.AddRange(groceriesExpense, rentExpense, income);
            await seedContext.SaveChangesAsync();
        }

        await using var queryContext = _fixture.CreateContext();
        IApplicationDbContext appContext = queryContext;
        var handler = new GetSpendByCategoryQueryHandler(appContext, new TestCurrentUserService("user-1"));

        var result = await handler.Handle(new GetSpendByCategoryQuery(2026, 4), CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.Equal("Rent", result[0].CategoryName);
        Assert.Equal(1500m, result[0].TotalSpent);
        Assert.Equal("Groceries", result[1].CategoryName);
        Assert.Equal(150m, result[1].TotalSpent);
    }
}
