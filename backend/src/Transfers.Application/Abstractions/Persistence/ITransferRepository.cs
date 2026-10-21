using Transfers.Domain.Limits;
using Transfers.Domain.Transfers;

namespace Transfers.Application.Abstractions.Persistence;

public interface ITransferRepository
{
    Task<Transfer?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<Transfer?> LockAsync(Guid id, CancellationToken cancellationToken);

    Task<Transfer?> LockNextDueAsync(DateTimeOffset now, CancellationToken cancellationToken);

    Task<Transfer?> GetByIdempotencyKeyAsync(string idempotencyKey, CancellationToken cancellationToken);

    Task<IReadOnlyList<Transfer>> ListByAccountAsync(Guid accountId, CancellationToken cancellationToken);

    Task<HourlyUsage> GetHourlyUsageAsync(Guid sourceAccountId, DateTimeOffset since, CancellationToken cancellationToken);

    void Add(Transfer transfer);
}
