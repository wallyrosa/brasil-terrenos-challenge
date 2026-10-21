namespace Transfers.Application.Transfers.ScheduleTransfer;

public sealed record ScheduleTransferCommand(
    Guid SourceAccountId,
    Guid DestinationAccountId,
    decimal Amount,
    DateTimeOffset ScheduledFor,
    string? IdempotencyKey);
