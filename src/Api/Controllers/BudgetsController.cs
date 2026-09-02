using Application.Features.Budgets;
using Application.Features.Budgets.Commands.CreateBudget;
using Application.Features.Budgets.Commands.DeleteBudget;
using Application.Features.Budgets.Commands.UpdateBudget;
using Application.Features.Budgets.Queries.GetBudgets;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class BudgetsController : ControllerBase
{
    private readonly IMediator _mediator;

    public BudgetsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<List<BudgetDto>>> GetBudgets([FromQuery] int year, [FromQuery] int month)
    {
        return Ok(await _mediator.Send(new GetBudgetsQuery(year, month)));
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> Create(CreateBudgetCommand command)
    {
        var id = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetBudgets), new { year = command.Year, month = command.Month }, id);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateBudgetCommand command)
    {
        if (id != command.Id)
        {
            return BadRequest();
        }

        await _mediator.Send(command);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _mediator.Send(new DeleteBudgetCommand(id));
        return NoContent();
    }
}
