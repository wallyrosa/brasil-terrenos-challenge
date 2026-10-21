using Microsoft.AspNetCore.Mvc;
using Transfers.Api.Contracts.Accounts;
using Transfers.Application.Accounts;
using Transfers.Application.Accounts.GetAccount;
using Transfers.Application.Accounts.ListAccounts;
using Transfers.Application.Accounts.OpenAccount;
using Transfers.Application.Accounts.UpdateAccount;

namespace Transfers.Api.Controllers;

[ApiController]
[Route("api/accounts")]
[Produces("application/json")]
public sealed class AccountsController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<AccountResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> List(
        [FromServices] ListAccountsHandler handler,
        CancellationToken cancellationToken) =>
        Ok(await handler.HandleAsync(cancellationToken));

    [HttpGet("{id:guid}")]
    [ProducesResponseType<AccountResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        Guid id,
        [FromServices] GetAccountHandler handler,
        CancellationToken cancellationToken) =>
        Ok(await handler.HandleAsync(id, cancellationToken));

    [HttpPost]
    [ProducesResponseType<AccountResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Open(
        OpenAccountRequest request,
        [FromServices] OpenAccountHandler handler,
        CancellationToken cancellationToken)
    {
        var account = await handler.HandleAsync(request.ToCommand(), cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = account.Id }, account);
    }

    [HttpPut("{id:guid}/overdraft-limit")]
    [ProducesResponseType<AccountResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> ChangeOverdraftLimit(
        Guid id,
        ChangeOverdraftLimitRequest request,
        [FromServices] UpdateAccountHandler handler,
        CancellationToken cancellationToken) =>
        Ok(await handler.ChangeOverdraftLimitAsync(id, request.OverdraftLimit, cancellationToken));

    [HttpPut("{id:guid}/transfer-limits")]
    [ProducesResponseType<AccountResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> ChangeTransferLimits(
        Guid id,
        ChangeTransferLimitsRequest request,
        [FromServices] UpdateAccountHandler handler,
        CancellationToken cancellationToken) =>
        Ok(await handler.ChangeTransferLimitsAsync(id, request.ToTransferLimits(), cancellationToken));

    [HttpPost("{id:guid}/activate")]
    [ProducesResponseType<AccountResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public Task<IActionResult> Activate(Guid id, [FromServices] UpdateAccountHandler handler, CancellationToken cancellationToken) =>
        ChangeStatus(id, AccountStatusAction.Activate, handler, cancellationToken);

    [HttpPost("{id:guid}/block")]
    [ProducesResponseType<AccountResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public Task<IActionResult> Block(Guid id, [FromServices] UpdateAccountHandler handler, CancellationToken cancellationToken) =>
        ChangeStatus(id, AccountStatusAction.Block, handler, cancellationToken);

    [HttpPost("{id:guid}/deactivate")]
    [ProducesResponseType<AccountResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public Task<IActionResult> Deactivate(Guid id, [FromServices] UpdateAccountHandler handler, CancellationToken cancellationToken) =>
        ChangeStatus(id, AccountStatusAction.Deactivate, handler, cancellationToken);

    private async Task<IActionResult> ChangeStatus(
        Guid id,
        AccountStatusAction action,
        UpdateAccountHandler handler,
        CancellationToken cancellationToken) =>
        Ok(await handler.ChangeStatusAsync(id, action, cancellationToken));
}
