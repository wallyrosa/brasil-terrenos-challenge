using Transfers.Application.Abstractions.Persistence;
using Transfers.Application.Transfers.Execution;
using Transfers.Domain.Transfers;

namespace Transfers.Application.Transfers.CreateTransfer;

public sealed class CreateTransferHandler(
    ITransferRepository transfers,
    IUnitOfWork unitOfWork,
    TransferPartiesLocker partiesLocker,
    IdempotencyGuard idempotencyGuard,
    TransferExecutor executor,
    TimeProvider timeProvider)
{
    public Task<TransferResponse> HandleAsync(CreateTransferCommand command, CancellationToken cancellationToken) =>
        unitOfWork.ExecuteInTransactionAsync(async token =>
        {
            var now = timeProvider.GetUtcNow();

            var transfer = Transfer.CreateImmediate(
                command.SourceAccountId,
                command.DestinationAccountId,
                command.Amount,
                command.IdempotencyKey,
                now);

            var parties = await partiesLocker.LockAsync(transfer.SourceAccountId, transfer.DestinationAccountId, token);

            var replay = await idempotencyGuard.FindReplayAsync(transfer, token);
            if (replay is not null)
                return TransferResponse.From(replay);

            await executor.ExecuteAsync(transfer, parties, now, token);

            transfers.Add(transfer);
            await unitOfWork.SaveChangesAsync(token);

            return TransferResponse.From(transfer);
        }, cancellationToken);
}
