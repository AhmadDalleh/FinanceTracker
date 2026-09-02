using MediatR;

namespace Application.Features.Reports.Queries.GetSpendByCategory;

public record GetSpendByCategoryQuery(int Year, int Month) : IRequest<List<CategorySpendDto>>;
