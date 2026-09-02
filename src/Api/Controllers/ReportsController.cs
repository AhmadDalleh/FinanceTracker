using Application.Features.Reports;
using Application.Features.Reports.Queries.GetMonthlySummary;
using Application.Features.Reports.Queries.GetSpendByCategory;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ReportsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("monthly-summary")]
    public async Task<ActionResult<MonthlySummaryDto>> GetMonthlySummary([FromQuery] int year, [FromQuery] int month)
    {
        return Ok(await _mediator.Send(new GetMonthlySummaryQuery(year, month)));
    }

    [HttpGet("spend-by-category")]
    public async Task<ActionResult<List<CategorySpendDto>>> GetSpendByCategory([FromQuery] int year, [FromQuery] int month)
    {
        return Ok(await _mediator.Send(new GetSpendByCategoryQuery(year, month)));
    }
}
