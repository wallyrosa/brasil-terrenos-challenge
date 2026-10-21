namespace Transfers.Application.Transfers.CreateTransfer;

public sealed record CreateTransferCommand(
    Guid SourceAccountId,
    Guid DestinationAccountId,
    decimal Amount,
    string? IdempotencyKey);
