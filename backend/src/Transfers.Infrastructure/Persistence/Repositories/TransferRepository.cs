using Microsoft.EntityFrameworkCore;
using Transfers.Application.Abstractions.Persistence;
using Transfers.Domain.Limits;
using Transfers.Domain.Transfers;

namespace Transfers.Infrastructure.Persistence.Repositories;

internal sealed class TransferRepository(TransfersDbContext context) : ITransferRepository
{
    private const int MaxListSize = 200;

    public Task<Transfer?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        context.Transfers.AsNoTracking().SingleOrDefaultAsync(transfer => transfer.Id == id, cancellationToken);

    public async Task<Transfer?> LockAsync(Guid id, CancellationToken cancellationToken)
    {
        var locked = await context.Transfers
            .FromSql($"SELECT * FROM transfers WHERE id = {id} FOR UPDATE")
            .ToListAsync(cancellationToken);

        return locked.SingleOrDefault();
    }

    public async Task<Transfer?> LockNextDueAsync(DateTimeOffset now, CancellationToken cancellationToken)
    {
        var scheduled = nameof(TransferStatus.Scheduled);

        var due = await context.Transfers
            .FromSql($"""
                SELECT * FROM transfers
                WHERE status = {scheduled} AND scheduled_for <= {now}
                ORDER BY scheduled_for
                LIMIT 1
                FOR UPDATE SKIP LOCKED
                """)
            .ToListAsync(cancellationToken);

        return due.SingleOrDefault();
    }

    public Task<Transfer?> GetByIdempotencyKeyAsync(string idempotencyKey, CancellationToken cancellationToken) =>
        context.Transfers
            .AsNoTracking()
            .SingleOrDefaultAsync(transfer => transfer.IdempotencyKey == idempotencyKey, cancellationToken);

    public async Task<IReadOnlyList<Transfer>> ListByAccountAsync(Guid accountId, CancellationToken cancellationToken) =>
        await context.Transfers
            .AsNoTracking()
            .Where(transfer => transfer.SourceAccountId == accountId || transfer.DestinationAccountId == accountId)
            .OrderByDescending(transfer => transfer.CreatedAt)
            .Take(MaxListSize)
            .ToListAsync(cancellationToken);

    public async Task<HourlyUsage> GetHourlyUsageAsync(
        Guid sourceAccountId,
        DateTimeOffset since,
        CancellationToken cancellationToken)
    {
        var attemptsInWindow = context.Transfers.Where(transfer =>
            transfer.SourceAccountId == sourceAccountId
            && transfer.ProcessedAt > since
            && (transfer.Status == TransferStatus.Completed || transfer.Status == TransferStatus.Failed));

        var attempts = await attemptsInWindow.CountAsync(cancellationToken);

        var transferredAmount = await attemptsInWindow
            .Where(transfer => transfer.Status == TransferStatus.Completed)
            .SumAsync(transfer => transfer.Amount, cancellationToken);

        return new HourlyUsage(transferredAmount, attempts);
    }

    public void Add(Transfer transfer) => context.Transfers.Add(transfer);
}
