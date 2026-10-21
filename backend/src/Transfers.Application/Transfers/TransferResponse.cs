using Transfers.Domain.Transfers;

namespace Transfers.Application.Transfers;

public sealed record TransferResponse(
    Guid Id,
    Guid SourceAccountId,
    Guid DestinationAccountId,
    decimal Amount,
    TransferType Type,
    TransferStatus Status,
    DateTimeOffset? ScheduledFor,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ProcessedAt,
    DateTimeOffset? CancelledAt,
    TransferFailureReason? FailureReason)
{
    public static TransferResponse From(Transfer transfer) =>
        new(
            transfer.Id,
            transfer.SourceAccountId,
            transfer.DestinationAccountId,
            transfer.Amount,
            transfer.Type,
            transfer.Status,
            transfer.ScheduledFor,
            transfer.CreatedAt,
            transfer.ProcessedAt,
            transfer.CancelledAt,
            transfer.FailureReason);
}
