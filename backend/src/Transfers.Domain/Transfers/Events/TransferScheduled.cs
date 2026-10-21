using Transfers.Domain.Common;

namespace Transfers.Domain.Transfers.Events;

public sealed record TransferScheduled(
    Guid TransferId,
    Guid SourceAccountId,
    Guid DestinationAccountId,
    decimal Amount,
    DateTimeOffset ScheduledFor,
    DateTimeOffset OccurredAt) : IDomainEvent;
