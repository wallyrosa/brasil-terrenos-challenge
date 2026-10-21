using Transfers.Domain.Common;

namespace Transfers.Domain.Transfers.Events;

public sealed record TransferFailed(
    Guid TransferId,
    Guid SourceAccountId,
    TransferFailureReason Reason,
    DateTimeOffset OccurredAt) : IDomainEvent;
