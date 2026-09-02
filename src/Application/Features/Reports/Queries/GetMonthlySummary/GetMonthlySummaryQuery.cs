using MediatR;

namespace Application.Features.Reports.Queries.GetMonthlySummary;

public record GetMonthlySummaryQuery(int Year, int Month) : IRequest<MonthlySummaryDto>;
