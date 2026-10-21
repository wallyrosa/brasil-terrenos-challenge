using System.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Transfers.Application.Abstractions.Events;
using Transfers.Application.Abstractions.Persistence;
using Transfers.Application.Common.Exceptions;
using Transfers.Domain.Common;

namespace Transfers.Infrastructure.Persistence;

internal sealed class UnitOfWork(TransfersDbContext context, IDomainEventDispatcher dispatcher) : IUnitOfWork
{
    private readonly List<IDomainEvent> _pendingEvents = [];

    public async Task<TResult> ExecuteInTransactionAsync<TResult>(
        Func<CancellationToken, Task<TResult>> operation,
        CancellationToken cancellationToken)
    {
        if (context.Database.CurrentTransaction is not null)
            return await operation(cancellationToken);

        try
        {
            TResult result;

            await using (var transaction = await context.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken))
            {
                result = await operation(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }

            await PublishPendingEventsAsync(cancellationToken);

            return result;
        }
        catch
        {
            _pendingEvents.Clear();
            context.ChangeTracker.Clear();
            throw;
        }
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        CollectDomainEvents();

        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            _pendingEvents.Clear();
            throw new ConflictException("unique_violation", "O registro conflita com um registro já existente.");
        }

        if (context.Database.CurrentTransaction is null)
            await PublishPendingEventsAsync(cancellationToken);
    }

    private void CollectDomainEvents()
    {
        var aggregates = context.ChangeTracker
            .Entries<AggregateRoot>()
            .Select(entry => entry.Entity)
            .Where(aggregate => aggregate.DomainEvents.Count > 0)
            .ToList();

        foreach (var aggregate in aggregates)
        {
            _pendingEvents.AddRange(aggregate.DomainEvents);
            aggregate.ClearDomainEvents();
        }
    }

    private async Task PublishPendingEventsAsync(CancellationToken cancellationToken)
    {
        if (_pendingEvents.Count == 0)
            return;

        var events = _pendingEvents.ToList();
        _pendingEvents.Clear();

        await dispatcher.DispatchAsync(events, cancellationToken);
    }
}
