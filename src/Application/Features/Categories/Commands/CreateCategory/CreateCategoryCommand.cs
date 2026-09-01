using MediatR;

namespace Application.Features.Categories.Commands.CreateCategory;

public record CreateCategoryCommand : IRequest<Guid>
{
    public required string Name { get; init; }
}
