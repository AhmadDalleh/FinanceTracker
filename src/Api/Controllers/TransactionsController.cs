using Application.Common.Models;
using Application.Features.Transactions;
using Application.Features.Transactions.Commands.CreateTransaction;
using Application.Features.Transactions.Commands.DeleteTransaction;
using Application.Features.Transactions.Commands.UpdateTransaction;
using Application.Features.Transactions.Queries.GetTransactionById;
using Application.Features.Transactions.Queries.GetTransactions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TransactionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public TransactionsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<PaginatedList<TransactionDto>>> GetTransactions([FromQuery] GetTransactionsQuery query)
    {
        return Ok(await _mediator.Send(query));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TransactionDto>> GetById(Guid id)
    {
        return Ok(await _mediator.Send(new GetTransactionByIdQuery(id)));
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> Create(CreateTransactionCommand command)
    {
        var id = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateTransactionCommand command)
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
        await _mediator.Send(new DeleteTransactionCommand(id));
        return NoContent();
    }
}
