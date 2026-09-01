using Application.Features.Categories.Commands.CreateCategory;
using Xunit;

namespace Application.UnitTests.Features.Categories.Commands;

public class CreateCategoryCommandValidatorTests
{
    private readonly CreateCategoryCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_HasNoErrors()
    {
        var result = _validator.Validate(new CreateCategoryCommand { Name = "Groceries" });

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WithEmptyName_HasError()
    {
        var result = _validator.Validate(new CreateCategoryCommand { Name = string.Empty });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateCategoryCommand.Name));
    }
}
