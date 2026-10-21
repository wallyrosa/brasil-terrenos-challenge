using Transfers.Application.Abstractions.Persistence;
using Transfers.Application.Common.Exceptions;
using Transfers.Domain.Transfers;

namespace Transfers.Application.Transfers.Execution;

public sealed class IdempotencyGuard(ITransferRepository transfers)
{
    public async Task<Transfer?> FindReplayAsync(Transfer candidate, CancellationToken cancellationToken)
    {
        if (candidate.IdempotencyKey is null)
            return null;

        var existing = await transfers.GetByIdempotencyKeyAsync(candidate.IdempotencyKey, cancellationToken);

        if (existing is null)
            return null;

        if (!IsSameRequest(existing, candidate))
            throw ApplicationErrors.IdempotencyKeyReused(candidate.IdempotencyKey);

        return existing;
    }

    private static bool IsSameRequest(Transfer existing, Transfer candidate) =>
        existing.SourceAccountId == candidate.SourceAccountId
        && existing.DestinationAccountId == candidate.DestinationAccountId
        && existing.Amount == candidate.Amount
        && existing.Type == candidate.Type
        && existing.ScheduledFor == candidate.ScheduledFor;
}
