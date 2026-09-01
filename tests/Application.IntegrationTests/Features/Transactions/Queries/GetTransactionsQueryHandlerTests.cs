using Application.Common.Interfaces;
using Application.Features.Transactions.Queries.GetTransactions;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using Domain.ValueObjects;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Application.IntegrationTests.Features.Transactions.Queries;

[Collection(DatabaseCollection.Name)]
public class GetTransactionsQueryHandlerTests : IAsyncLifetime
{
    private readonly PostgresContainerFixture _fixture;
    private readonly IMapper _mapper;

    public GetTransactionsQueryHandlerTests(PostgresContainerFixture fixture)
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
        context.Transactions.RemoveRange(context.Transactions);
        context.Categories.RemoveRange(context.Categories);
        context.Accounts.RemoveRange(context.Accounts);
        await context.SaveChangesAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    private static Account NewAccount(string userId) => new()
    {
        Id = Guid.NewGuid(),
        UserId = userId,
        Name = "Checking",
        Type = AccountType.Checking,
        Balance = new Money(0m, "USD")
    };

    private static Category NewCategory(string userId, string name) => new()
    {
        Id = Guid.NewGuid(),
        UserId = userId,
        Name = name
    };

    [Fact]
    public async Task Handle_FiltersToOwnedAccountsOnly()
    {
        var myAccount = NewAccount("user-1");
        var otherAccount = NewAccount("user-2");
        var myCategory = NewCategory("user-1", "Groceries");
        var otherCategory = NewCategory("user-2", "Groceries");

        var myTransaction = new Transaction { Id = Guid.NewGuid(), AccountId = myAccount.Id, CategoryId = myCategory.Id, Amount = 25m, Type = TransactionType.Expense, Date = new DateOnly(2026, 1, 10) };
        var otherTransaction = new Transaction { Id = Guid.NewGuid(), AccountId = otherAccount.Id, CategoryId = otherCategory.Id, Amount = 25m, Type = TransactionType.Expense, Date = new DateOnly(2026, 1, 10) };

        await using (var seedContext = _fixture.CreateContext())
        {
            seedContext.Accounts.AddRange(myAccount, otherAccount);
            seedContext.Categories.AddRange(myCategory, otherCategory);
            seedContext.Transactions.AddRange(myTransaction, otherTransaction);
            await seedContext.SaveChangesAsync();
        }

        await using var queryContext = _fixture.CreateContext();
        IApplicationDbContext appContext = queryContext;
        var handler = new GetTransactionsQueryHandler(appContext, new TestCurrentUserService("user-1"), _mapper);

        var result = await handler.Handle(new GetTransactionsQuery(), CancellationToken.None);

        var item = Assert.Single(result.Items);
        Assert.Equal(myTransaction.Id, item.Id);
    }

    [Fact]
    public async Task Handle_FiltersByDateRangeAndAmount()
    {
        var account = NewAccount("user-1");
        var category = NewCategory("user-1", "Groceries");

        var inRange = new Transaction { Id = Guid.NewGuid(), AccountId = account.Id, CategoryId = category.Id, Amount = 40m, Type = TransactionType.Expense, Date = new DateOnly(2026, 2, 5) };
        var beforeRange = new Transaction { Id = Guid.NewGuid(), AccountId = account.Id, CategoryId = category.Id, Amount = 40m, Type = TransactionType.Expense, Date = new DateOnly(2026, 1, 1) };
        var tooExpensive = new Transaction { Id = Guid.NewGuid(), AccountId = account.Id, CategoryId = category.Id, Amount = 500m, Type = TransactionType.Expense, Date = new DateOnly(2026, 2, 6) };

        await using (var seedContext = _fixture.CreateContext())
        {
            seedContext.Accounts.Add(account);
            seedContext.Categories.Add(category);
            seedContext.Transactions.AddRange(inRange, beforeRange, tooExpensive);
            await seedContext.SaveChangesAsync();
        }

        await using var queryContext = _fixture.CreateContext();
        IApplicationDbContext appContext = queryContext;
        var handler = new GetTransactionsQueryHandler(appContext, new TestCurrentUserService("user-1"), _mapper);

        var query = new GetTransactionsQuery
        {
            FromDate = new DateOnly(2026, 2, 1),
            ToDate = new DateOnly(2026, 2, 28),
            MaxAmount = 100m
        };
        var result = await handler.Handle(query, CancellationToken.None);

        var item = Assert.Single(result.Items);
        Assert.Equal(inRange.Id, item.Id);
    }

    [Fact]
    public async Task Handle_PaginatesResults()
    {
        var account = NewAccount("user-1");
        var category = NewCategory("user-1", "Groceries");

        var transactions = Enumerable.Range(1, 5)
            .Select(i => new Transaction
            {
                Id = Guid.NewGuid(),
                AccountId = account.Id,
                CategoryId = category.Id,
                Amount = 10m * i,
                Type = TransactionType.Expense,
                Date = new DateOnly(2026, 1, i)
            })
            .ToList();

        await using (var seedContext = _fixture.CreateContext())
        {
            seedContext.Accounts.Add(account);
            seedContext.Categories.Add(category);
            seedContext.Transactions.AddRange(transactions);
            await seedContext.SaveChangesAsync();
        }

        await using var queryContext = _fixture.CreateContext();
        IApplicationDbContext appContext = queryContext;
        var handler = new GetTransactionsQueryHandler(appContext, new TestCurrentUserService("user-1"), _mapper);

        var result = await handler.Handle(new GetTransactionsQuery { PageNumber = 1, PageSize = 2 }, CancellationToken.None);

        Assert.Equal(2, result.Items.Count);
        Assert.Equal(5, result.TotalCount);
        Assert.Equal(3, result.TotalPages);
    }
}
