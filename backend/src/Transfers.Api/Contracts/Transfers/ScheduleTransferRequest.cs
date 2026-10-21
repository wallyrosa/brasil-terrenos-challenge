using System.ComponentModel.DataAnnotations;
using Transfers.Application.Transfers.ScheduleTransfer;

namespace Transfers.Api.Contracts.Transfers;

public sealed record ScheduleTransferRequest(
    [Required] Guid? SourceAccountId,
    [Required] Guid? DestinationAccountId,
    [Required] decimal? Amount,
    [Required] DateTimeOffset? ScheduledFor)
{
    public ScheduleTransferCommand ToCommand(string? idempotencyKey) =>
        new(SourceAccountId!.Value, DestinationAccountId!.Value, Amount!.Value, ScheduledFor!.Value, idempotencyKey);
}
