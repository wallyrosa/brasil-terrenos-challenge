using Transfers.Application.Abstractions.Persistence;
using Transfers.Application.Transfers.Execution;
using Transfers.Domain.Common;
using Transfers.Domain.Transfers;

namespace Transfers.Application.Transfers.ScheduleTransfer;

public sealed class ScheduleTransferHandler(
    ITransferRepository transfers,
    IUnitOfWork unitOfWork,
    TransferPartiesLocker partiesLocker,
    IdempotencyGuard idempotencyGuard,
    TimeProvider timeProvider)
{
    public Task<TransferResponse> HandleAsync(ScheduleTransferCommand command, CancellationToken cancellationToken) =>
        unitOfWork.ExecuteInTransactionAsync(async token =>
        {
            var transfer = Transfer.Schedule(
                command.SourceAccountId,
                command.DestinationAccountId,
                command.Amount,
                command.ScheduledFor,
                command.IdempotencyKey,
                timeProvider.GetUtcNow());

            var parties = await partiesLocker.LockAsync(transfer.SourceAccountId, transfer.DestinationAccountId, token);

            var replay = await idempotencyGuard.FindReplayAsync(transfer, token);
            if (replay is not null)
                return TransferResponse.From(replay);

            EnsureActive(parties);

            transfers.Add(transfer);
            await unitOfWork.SaveChangesAsync(token);

            return TransferResponse.From(transfer);
        }, cancellationToken);

    private static void EnsureActive(TransferParties parties)
    {
        if (!parties.Source.IsActive)
            throw DomainErrors.AccountNotActive(parties.Source.Id);

        if (!parties.Destination.IsActive)
            throw DomainErrors.AccountNotActive(parties.Destination.Id);
    }
}
