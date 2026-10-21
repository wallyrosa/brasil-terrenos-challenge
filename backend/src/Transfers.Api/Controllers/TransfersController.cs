using Microsoft.AspNetCore.Mvc;
using Transfers.Api.Contracts;
using Transfers.Api.Contracts.Transfers;
using Transfers.Api.ErrorHandling;
using Transfers.Application.Transfers;
using Transfers.Application.Transfers.CancelTransfer;
using Transfers.Application.Transfers.CreateTransfer;
using Transfers.Application.Transfers.GetTransfer;
using Transfers.Application.Transfers.ListTransfers;
using Transfers.Application.Transfers.ScheduleTransfer;
using Transfers.Domain.Transfers;

namespace Transfers.Api.Controllers;

[ApiController]
[Route("api/transfers")]
[Produces("application/json")]
public sealed class TransfersController : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<TransferResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Create(
        CreateTransferRequest request,
        [FromHeader(Name = Headers.IdempotencyKey)] string? idempotencyKey,
        [FromServices] CreateTransferHandler handler,
        CancellationToken cancellationToken)
    {
        var transfer = await handler.HandleAsync(request.ToCommand(idempotencyKey), cancellationToken);

        return transfer.Status == TransferStatus.Failed
            ? UnprocessableEntity(TransferRejectionProblem.From(transfer))
            : CreatedAtAction(nameof(GetById), new { id = transfer.Id }, transfer);
    }

    [HttpPost("scheduled")]
    [ProducesResponseType<TransferResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Schedule(
        ScheduleTransferRequest request,
        [FromHeader(Name = Headers.IdempotencyKey)] string? idempotencyKey,
        [FromServices] ScheduleTransferHandler handler,
        CancellationToken cancellationToken)
    {
        var transfer = await handler.HandleAsync(request.ToCommand(idempotencyKey), cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = transfer.Id }, transfer);
    }

    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType<TransferResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Cancel(
        Guid id,
        [FromServices] CancelTransferHandler handler,
        CancellationToken cancellationToken) =>
        Ok(await handler.HandleAsync(id, cancellationToken));

    [HttpGet("{id:guid}")]
    [ProducesResponseType<TransferResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        Guid id,
        [FromServices] GetTransferHandler handler,
        CancellationToken cancellationToken) =>
        Ok(await handler.HandleAsync(id, cancellationToken));

    [HttpGet]
    [ProducesResponseType<IReadOnlyList<TransferResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ListByAccount(
        [FromQuery] Guid accountId,
        [FromServices] ListTransfersHandler handler,
        CancellationToken cancellationToken) =>
        Ok(await handler.HandleAsync(accountId, cancellationToken));
}
