using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Features.Accounts.Queries.GetAccountById;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using Domain.ValueObjects;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Application.IntegrationTests.Features.Accounts.Queries;

[Collection(DatabaseCollection.Name)]
public class GetAccountByIdQueryHandlerTests : IAsyncLifetime
{
    private readonly PostgresContainerFixture _fixture;
    private readonly IMapper _mapper;

    public GetAccountByIdQueryHandlerTests(PostgresContainerFixture fixture)
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
    public async Task Handle_WhenAccountBelongsToCurrentUser_ReturnsAccount()
    {
        var account = new Account
        {
            Id = Guid.NewGuid(),
            UserId = "user-1",
            Name = "Checking",
            Type = AccountType.Checking,
            Balance = new Money(100m, "USD")
        };

        await using (var seedContext = _fixture.CreateContext())
        {
            seedContext.Accounts.Add(account);
            await seedContext.SaveChangesAsync();
        }

        await using var queryContext = _fixture.CreateContext();
        IApplicationDbContext appContext = queryContext;
        var handler = new GetAccountByIdQueryHandler(appContext, new TestCurrentUserService("user-1"), _mapper);

        var result = await handler.Handle(new GetAccountByIdQuery(account.Id), CancellationToken.None);

        Assert.Equal(account.Id, result.Id);
        Assert.Equal("Checking", result.Name);
    }

    [Fact]
    public async Task Handle_WhenAccountBelongsToAnotherUser_ThrowsNotFoundException()
    {
        var account = new Account
        {
            Id = Guid.NewGuid(),
            UserId = "someone-else",
            Name = "Checking",
            Type = AccountType.Checking,
            Balance = new Money(100m, "USD")
        };

        await using (var seedContext = _fixture.CreateContext())
        {
            seedContext.Accounts.Add(account);
            await seedContext.SaveChangesAsync();
        }

        await using var queryContext = _fixture.CreateContext();
        IApplicationDbContext appContext = queryContext;
        var handler = new GetAccountByIdQueryHandler(appContext, new TestCurrentUserService("user-1"), _mapper);

        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(new GetAccountByIdQuery(account.Id), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenAccountDoesNotExist_ThrowsNotFoundException()
    {
        await using var queryContext = _fixture.CreateContext();
        IApplicationDbContext appContext = queryContext;
        var handler = new GetAccountByIdQueryHandler(appContext, new TestCurrentUserService("user-1"), _mapper);

        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(new GetAccountByIdQuery(Guid.NewGuid()), CancellationToken.None));
    }
}
