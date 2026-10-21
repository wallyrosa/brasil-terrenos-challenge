using Microsoft.Extensions.Logging;
using Transfers.Application.Abstractions.Events;
using Transfers.Domain.Transfers.Events;

namespace Transfers.Application.Transfers.EventHandlers;

public sealed class TransferAuditLogHandler(ILogger<TransferAuditLogHandler> logger) :
    IDomainEventHandler<TransferScheduled>,
    IDomainEventHandler<TransferCompleted>,
    IDomainEventHandler<TransferFailed>,
    IDomainEventHandler<TransferCancelled>
{
    public Task HandleAsync(TransferScheduled domainEvent, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Transfer {TransferId} of {Amount} from {SourceAccountId} to {DestinationAccountId} scheduled for {ScheduledFor}",
            domainEvent.TransferId,
            domainEvent.Amount,
            domainEvent.SourceAccountId,
            domainEvent.DestinationAccountId,
            domainEvent.ScheduledFor);

        return Task.CompletedTask;
    }

    public Task HandleAsync(TransferCompleted domainEvent, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Transfer {TransferId} of {Amount} from {SourceAccountId} to {DestinationAccountId} completed",
            domainEvent.TransferId,
            domainEvent.Amount,
            domainEvent.SourceAccountId,
            domainEvent.DestinationAccountId);

        return Task.CompletedTask;
    }

    public Task HandleAsync(TransferFailed domainEvent, CancellationToken cancellationToken)
    {
        logger.LogWarning(
            "Transfer {TransferId} from {SourceAccountId} failed: {Reason}",
            domainEvent.TransferId,
            domainEvent.SourceAccountId,
            domainEvent.Reason);

        return Task.CompletedTask;
    }

    public Task HandleAsync(TransferCancelled domainEvent, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Transfer {TransferId} from {SourceAccountId} cancelled",
            domainEvent.TransferId,
            domainEvent.SourceAccountId);

        return Task.CompletedTask;
    }
}
