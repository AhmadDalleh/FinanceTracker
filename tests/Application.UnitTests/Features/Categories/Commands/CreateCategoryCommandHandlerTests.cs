using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Features.Categories.Commands.CreateCategory;
using Domain.Entities;
using Moq;
using Xunit;

namespace Application.UnitTests.Features.Categories.Commands;

public class CreateCategoryCommandHandlerTests
{
    private readonly Mock<ICategoryRepository> _categoryRepository = new();
    private readonly Mock<IApplicationDbContext> _context = new();
    private readonly Mock<ICurrentUserService> _currentUserService = new();
    private readonly CreateCategoryCommandHandler _handler;

    public CreateCategoryCommandHandlerTests()
    {
        _handler = new CreateCategoryCommandHandler(_categoryRepository.Object, _context.Object, _currentUserService.Object);
    }

    [Fact]
    public async Task Handle_WithAuthenticatedUser_AddsCategoryAndSaves()
    {
        _currentUserService.Setup(s => s.UserId).Returns("user-1");

        var id = await _handler.Handle(new CreateCategoryCommand { Name = "Groceries" }, CancellationToken.None);

        Assert.NotEqual(Guid.Empty, id);
        _categoryRepository.Verify(r => r.AddAsync(
            It.Is<Category>(c => c.Id == id && c.UserId == "user-1" && c.Name == "Groceries"),
            It.IsAny<CancellationToken>()), Times.Once);
        _context.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithoutAuthenticatedUser_ThrowsForbiddenAccessException()
    {
        _currentUserService.Setup(s => s.UserId).Returns((string?)null);

        await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            _handler.Handle(new CreateCategoryCommand { Name = "Groceries" }, CancellationToken.None));
    }
}
