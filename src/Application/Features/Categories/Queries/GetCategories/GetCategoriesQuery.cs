using MediatR;

namespace Application.Features.Categories.Queries.GetCategories;

public record GetCategoriesQuery : IRequest<List<CategoryDto>>;
