using Application.Common.Interfaces;
using Application.Features.Accounts.Queries.GetAccounts;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using Domain.ValueObjects;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Application.IntegrationTests.Features.Accounts.Queries;

[Collection(DatabaseCollection.Name)]
public class GetAccountsQueryHandlerTests : IAsyncLifetime
{
    private readonly PostgresContainerFixture _fixture;
    private readonly IMapper _mapper;

    public GetAccountsQueryHandlerTests(PostgresContainerFixture fixture)
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
        context.Accounts.RemoveRange(context.Accounts);
        await context.SaveChangesAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Handle_ReturnsOnlyCurrentUsersNonArchivedAccounts()
    {
        await using (var seedContext = _fixture.CreateContext())
        {
            seedContext.Accounts.AddRange(
                new Account { Id = Guid.NewGuid(), UserId = "user-1", Name = "Checking", Type = AccountType.Checking, Balance = new Money(100m, "USD") },
                new Account { Id = Guid.NewGuid(), UserId = "user-1", Name = "Archived", Type = AccountType.Savings, Balance = new Money(0m, "USD"), IsArchived = true },
                new Account { Id = Guid.NewGuid(), UserId = "user-2", Name = "Other User", Type = AccountType.Checking, Balance = new Money(50m, "USD") });
            await seedContext.SaveChangesAsync();
        }

        await using var queryContext = _fixture.CreateContext();
        IApplicationDbContext appContext = queryContext;
        var handler = new GetAccountsQueryHandler(appContext, new TestCurrentUserService("user-1"), _mapper);

        var result = await handler.Handle(new GetAccountsQuery(), CancellationToken.None);

        var account = Assert.Single(result);
        Assert.Equal("Checking", account.Name);
        Assert.Equal(100m, account.Balance);
        Assert.Equal("USD", account.Currency);
    }

    [Fact]
    public async Task Handle_WithIncludeArchivedTrue_ReturnsArchivedAccountsToo()
    {
        await using (var seedContext = _fixture.CreateContext())
        {
            seedContext.Accounts.AddRange(
                new Account { Id = Guid.NewGuid(), UserId = "user-1", Name = "Checking", Type = AccountType.Checking, Balance = new Money(100m, "USD") },
                new Account { Id = Guid.NewGuid(), UserId = "user-1", Name = "Archived", Type = AccountType.Savings, Balance = new Money(0m, "USD"), IsArchived = true });
            await seedContext.SaveChangesAsync();
        }

        await using var queryContext = _fixture.CreateContext();
        IApplicationDbContext appContext = queryContext;
        var handler = new GetAccountsQueryHandler(appContext, new TestCurrentUserService("user-1"), _mapper);

        var result = await handler.Handle(new GetAccountsQuery(IncludeArchived: true), CancellationToken.None);

        Assert.Equal(2, result.Count);
    }
}
