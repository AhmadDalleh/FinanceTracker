using System.Net.Http.Json;
using Application.Features.Categories;
using Application.Features.Categories.Commands.CreateCategory;
using Xunit;

namespace Api.FunctionalTests.Controllers;

public class CategoriesControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public CategoriesControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
        factory.EnsureDatabaseCreated();
    }

    [Fact]
    public async Task GetCategories_AfterCreatingOne_ReturnsItSortedByName()
    {
        await _client.PostAsJsonAsync("/api/Categories", new CreateCategoryCommand { Name = "Zoo" });
        await _client.PostAsJsonAsync("/api/Categories", new CreateCategoryCommand { Name = "Apple" });

        var response = await _client.GetAsync("/api/Categories");
        response.EnsureSuccessStatusCode();

        var categories = await response.Content.ReadFromJsonAsync<List<CategoryDto>>();

        Assert.NotNull(categories);
        Assert.Contains(categories!, c => c.Name == "Zoo");
        Assert.Contains(categories!, c => c.Name == "Apple");
        var appleIndex = categories!.FindIndex(c => c.Name == "Apple");
        var zooIndex = categories!.FindIndex(c => c.Name == "Zoo");
        Assert.True(appleIndex < zooIndex);
    }
}
