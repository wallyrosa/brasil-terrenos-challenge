using Transfers.Domain.Common;

namespace Transfers.Domain.Transfers.Events;

public sealed record TransferCompleted(
    Guid TransferId,
    Guid SourceAccountId,
    Guid DestinationAccountId,
    decimal Amount,
    DateTimeOffset OccurredAt) : IDomainEvent;
