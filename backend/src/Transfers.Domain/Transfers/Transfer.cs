using Transfers.Domain.Common;
using Transfers.Domain.Transfers.Events;

namespace Transfers.Domain.Transfers;

public sealed class Transfer : AggregateRoot
{
    public Guid SourceAccountId { get; private set; }

    public Guid DestinationAccountId { get; private set; }

    public decimal Amount { get; private set; }

    public TransferType Type { get; private set; }

    public TransferStatus Status { get; private set; }

    public DateTimeOffset? ScheduledFor { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset? ProcessedAt { get; private set; }

    public DateTimeOffset? CancelledAt { get; private set; }

    public TransferFailureReason? FailureReason { get; private set; }

    public string? IdempotencyKey { get; private set; }

    private Transfer()
    {
    }

    public static Transfer CreateImmediate(
        Guid sourceAccountId,
        Guid destinationAccountId,
        decimal amount,
        string? idempotencyKey,
        DateTimeOffset now) =>
        Create(
            sourceAccountId,
            destinationAccountId,
            amount,
            TransferType.Immediate,
            TransferStatus.Processing,
            null,
            idempotencyKey,
            now);

    public static Transfer Schedule(
        Guid sourceAccountId,
        Guid destinationAccountId,
        decimal amount,
        DateTimeOffset scheduledFor,
        string? idempotencyKey,
        DateTimeOffset now)
    {
        var executionDate = scheduledFor.ToUniversalTime();

        if (executionDate <= now)
            throw DomainErrors.ScheduleMustBeInFuture();

        var transfer = Create(
            sourceAccountId,
            destinationAccountId,
            amount,
            TransferType.Scheduled,
            TransferStatus.Scheduled,
            executionDate,
            idempotencyKey,
            now);

        transfer.Raise(new TransferScheduled(
            transfer.Id,
            sourceAccountId,
            destinationAccountId,
            amount,
            executionDate,
            now));

        return transfer;
    }

    public bool IsDue(DateTimeOffset now) =>
        Status == TransferStatus.Scheduled && ScheduledFor <= now;

    public bool Involves(Guid accountId) =>
        SourceAccountId == accountId || DestinationAccountId == accountId;

    public void StartProcessing(DateTimeOffset now)
    {
        EnsureStatus(TransferStatus.Scheduled, TransferStatus.Processing);

        if (!IsDue(now))
            throw DomainErrors.TransferNotDue();

        Status = TransferStatus.Processing;
    }

    public void Cancel(DateTimeOffset now)
    {
        EnsureStatus(TransferStatus.Scheduled, TransferStatus.Cancelled);

        Status = TransferStatus.Cancelled;
        CancelledAt = now;

        Raise(new TransferCancelled(Id, SourceAccountId, now));
    }

    internal void Complete(DateTimeOffset now)
    {
        EnsureStatus(TransferStatus.Processing, TransferStatus.Completed);

        Status = TransferStatus.Completed;
        ProcessedAt = now;

        Raise(new TransferCompleted(Id, SourceAccountId, DestinationAccountId, Amount, now));
    }

    internal void Fail(TransferFailureReason reason, DateTimeOffset now)
    {
        EnsureStatus(TransferStatus.Processing, TransferStatus.Failed);

        Status = TransferStatus.Failed;
        FailureReason = reason;
        ProcessedAt = now;

        Raise(new TransferFailed(Id, SourceAccountId, reason, now));
    }

    private static Transfer Create(
        Guid sourceAccountId,
        Guid destinationAccountId,
        decimal amount,
        TransferType type,
        TransferStatus status,
        DateTimeOffset? scheduledFor,
        string? idempotencyKey,
        DateTimeOffset now)
    {
        if (sourceAccountId == destinationAccountId)
            throw DomainErrors.SameSourceAndDestination();

        Money.EnsurePositive(amount);

        return new Transfer
        {
            Id = Guid.CreateVersion7(),
            SourceAccountId = sourceAccountId,
            DestinationAccountId = destinationAccountId,
            Amount = amount,
            Type = type,
            Status = status,
            ScheduledFor = scheduledFor,
            IdempotencyKey = string.IsNullOrWhiteSpace(idempotencyKey) ? null : idempotencyKey.Trim(),
            CreatedAt = now
        };
    }

    private void EnsureStatus(TransferStatus expected, TransferStatus target)
    {
        if (Status != expected)
            throw DomainErrors.InvalidTransferStatusTransition(Status.ToString(), target.ToString());
    }
}
