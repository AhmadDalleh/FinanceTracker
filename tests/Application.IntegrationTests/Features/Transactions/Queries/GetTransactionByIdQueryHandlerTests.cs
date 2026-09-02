using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Features.Transactions.Queries.GetTransactionById;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using Domain.ValueObjects;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Application.IntegrationTests.Features.Transactions.Queries;

[Collection(DatabaseCollection.Name)]
public class GetTransactionByIdQueryHandlerTests : IAsyncLifetime
{
    private readonly PostgresContainerFixture _fixture;
    private readonly IMapper _mapper;

    public GetTransactionByIdQueryHandlerTests(PostgresContainerFixture fixture)
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

    [Fact]
    public async Task Handle_WhenTransactionBelongsToCurrentUsersAccount_ReturnsIt()
    {
        var account = new Account { Id = Guid.NewGuid(), UserId = "user-1", Name = "Checking", Type = AccountType.Checking, Balance = new Money(0m, "USD") };
        var category = new Category { Id = Guid.NewGuid(), UserId = "user-1", Name = "Groceries" };
        var transaction = new Transaction { Id = Guid.NewGuid(), AccountId = account.Id, CategoryId = category.Id, Amount = 42m, Type = TransactionType.Expense, Date = new DateOnly(2026, 1, 25) };

        await using (var seedContext = _fixture.CreateContext())
        {
            seedContext.Accounts.Add(account);
            seedContext.Categories.Add(category);
            seedContext.Transactions.Add(transaction);
            await seedContext.SaveChangesAsync();
        }

        await using var queryContext = _fixture.CreateContext();
        IApplicationDbContext appContext = queryContext;
        var handler = new GetTransactionByIdQueryHandler(appContext, new TestCurrentUserService("user-1"), _mapper);

        var result = await handler.Handle(new GetTransactionByIdQuery(transaction.Id), CancellationToken.None);

        Assert.Equal(transaction.Id, result.Id);
        Assert.Equal(42m, result.Amount);
    }

    [Fact]
    public async Task Handle_WhenTransactionBelongsToAnotherUsersAccount_ThrowsNotFoundException()
    {
        var account = new Account { Id = Guid.NewGuid(), UserId = "someone-else", Name = "Checking", Type = AccountType.Checking, Balance = new Money(0m, "USD") };
        var category = new Category { Id = Guid.NewGuid(), UserId = "someone-else", Name = "Groceries" };
        var transaction = new Transaction { Id = Guid.NewGuid(), AccountId = account.Id, CategoryId = category.Id, Amount = 42m, Type = TransactionType.Expense, Date = new DateOnly(2026, 1, 25) };

        await using (var seedContext = _fixture.CreateContext())
        {
            seedContext.Accounts.Add(account);
            seedContext.Categories.Add(category);
            seedContext.Transactions.Add(transaction);
            await seedContext.SaveChangesAsync();
        }

        await using var queryContext = _fixture.CreateContext();
        IApplicationDbContext appContext = queryContext;
        var handler = new GetTransactionByIdQueryHandler(appContext, new TestCurrentUserService("user-1"), _mapper);

        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(new GetTransactionByIdQuery(transaction.Id), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenTransactionDoesNotExist_ThrowsNotFoundException()
    {
        await using var queryContext = _fixture.CreateContext();
        IApplicationDbContext appContext = queryContext;
        var handler = new GetTransactionByIdQueryHandler(appContext, new TestCurrentUserService("user-1"), _mapper);

        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(new GetTransactionByIdQuery(Guid.NewGuid()), CancellationToken.None));
    }
}
