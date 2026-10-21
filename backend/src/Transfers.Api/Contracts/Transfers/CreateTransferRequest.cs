using System.ComponentModel.DataAnnotations;
using Transfers.Application.Transfers.CreateTransfer;

namespace Transfers.Api.Contracts.Transfers;

public sealed record CreateTransferRequest(
    [Required] Guid? SourceAccountId,
    [Required] Guid? DestinationAccountId,
    [Required] decimal? Amount)
{
    public CreateTransferCommand ToCommand(string? idempotencyKey) =>
        new(SourceAccountId!.Value, DestinationAccountId!.Value, Amount!.Value, idempotencyKey);
}
