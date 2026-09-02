using Application.Common.Interfaces;
using Application.Features.Reports.Queries.GetMonthlySummary;
using Domain.Entities;
using Domain.Enums;
using Domain.ValueObjects;
using Xunit;

namespace Application.IntegrationTests.Features.Reports.Queries;

[Collection(DatabaseCollection.Name)]
public class GetMonthlySummaryQueryHandlerTests : IAsyncLifetime
{
    private readonly PostgresContainerFixture _fixture;

    public GetMonthlySummaryQueryHandlerTests(PostgresContainerFixture fixture)
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
    public async Task Handle_SumsIncomeAndExpenseForOwnedAccountsWithinMonth()
    {
        var myAccount = new Account { Id = Guid.NewGuid(), UserId = "user-1", Name = "Checking", Type = AccountType.Checking, Balance = new Money(0m, "USD") };
        var otherAccount = new Account { Id = Guid.NewGuid(), UserId = "user-2", Name = "Checking", Type = AccountType.Checking, Balance = new Money(0m, "USD") };
        var category = new Category { Id = Guid.NewGuid(), UserId = "user-1", Name = "Salary" };
        var otherCategory = new Category { Id = Guid.NewGuid(), UserId = "user-2", Name = "Salary" };

        var income = new Transaction { Id = Guid.NewGuid(), AccountId = myAccount.Id, CategoryId = category.Id, Amount = 3000m, Type = TransactionType.Income, Date = new DateOnly(2026, 3, 1) };
        var expense = new Transaction { Id = Guid.NewGuid(), AccountId = myAccount.Id, CategoryId = category.Id, Amount = 1200m, Type = TransactionType.Expense, Date = new DateOnly(2026, 3, 15) };
        var outOfMonth = new Transaction { Id = Guid.NewGuid(), AccountId = myAccount.Id, CategoryId = category.Id, Amount = 5000m, Type = TransactionType.Income, Date = new DateOnly(2026, 4, 1) };
        var otherUsersTransaction = new Transaction { Id = Guid.NewGuid(), AccountId = otherAccount.Id, CategoryId = otherCategory.Id, Amount = 9999m, Type = TransactionType.Income, Date = new DateOnly(2026, 3, 10) };

        await using (var seedContext = _fixture.CreateContext())
        {
            seedContext.Accounts.AddRange(myAccount, otherAccount);
            seedContext.Categories.AddRange(category, otherCategory);
            seedContext.Transactions.AddRange(income, expense, outOfMonth, otherUsersTransaction);
            await seedContext.SaveChangesAsync();
        }

        await using var queryContext = _fixture.CreateContext();
        IApplicationDbContext appContext = queryContext;
        var handler = new GetMonthlySummaryQueryHandler(appContext, new TestCurrentUserService("user-1"));

        var result = await handler.Handle(new GetMonthlySummaryQuery(2026, 3), CancellationToken.None);

        Assert.Equal(3000m, result.TotalIncome);
        Assert.Equal(1200m, result.TotalExpense);
        Assert.Equal(1800m, result.NetPosition);
    }
}
