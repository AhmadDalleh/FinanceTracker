using Application.Common.Interfaces;
using Application.Features.Categories.Queries.GetCategories;
using AutoMapper;
using Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Application.IntegrationTests.Features.Categories.Queries;

[Collection(DatabaseCollection.Name)]
public class GetCategoriesQueryHandlerTests : IAsyncLifetime
{
    private readonly PostgresContainerFixture _fixture;
    private readonly IMapper _mapper;

    public GetCategoriesQueryHandlerTests(PostgresContainerFixture fixture)
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
        // Transactions and Budgets both FK-reference Categories with
        // DeleteBehavior.Restrict, so they must go first - other test
        // classes sharing this same Postgres container may have left rows
        // behind that still reference categories this cleanup is about to
        // remove.
        context.Transactions.RemoveRange(context.Transactions);
        context.Budgets.RemoveRange(context.Budgets);
        context.Categories.RemoveRange(context.Categories);
        await context.SaveChangesAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Handle_ReturnsOnlyCurrentUsersCategoriesOrderedByName()
    {
        await using (var seedContext = _fixture.CreateContext())
        {
            seedContext.Categories.AddRange(
                new Category { Id = Guid.NewGuid(), UserId = "user-1", Name = "Zoo" },
                new Category { Id = Guid.NewGuid(), UserId = "user-1", Name = "Apple" },
                new Category { Id = Guid.NewGuid(), UserId = "user-2", Name = "Other User's Category" });
            await seedContext.SaveChangesAsync();
        }

        await using var queryContext = _fixture.CreateContext();
        IApplicationDbContext appContext = queryContext;
        var handler = new GetCategoriesQueryHandler(appContext, new TestCurrentUserService("user-1"), _mapper);

        var result = await handler.Handle(new GetCategoriesQuery(), CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.Equal("Apple", result[0].Name);
        Assert.Equal("Zoo", result[1].Name);
    }
}
