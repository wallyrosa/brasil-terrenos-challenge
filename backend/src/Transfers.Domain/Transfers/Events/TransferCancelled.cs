using Transfers.Domain.Common;

namespace Transfers.Domain.Transfers.Events;

public sealed record TransferCancelled(
    Guid TransferId,
    Guid SourceAccountId,
    DateTimeOffset OccurredAt) : IDomainEvent;
