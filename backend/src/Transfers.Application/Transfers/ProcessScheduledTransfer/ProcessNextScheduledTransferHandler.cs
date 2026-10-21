using Transfers.Application.Abstractions.Persistence;
using Transfers.Application.Transfers.Execution;

namespace Transfers.Application.Transfers.ProcessScheduledTransfer;

public sealed class ProcessNextScheduledTransferHandler(
    ITransferRepository transfers,
    IUnitOfWork unitOfWork,
    TransferPartiesLocker partiesLocker,
    TransferExecutor executor,
    TimeProvider timeProvider)
{
    public Task<TransferResponse?> HandleAsync(CancellationToken cancellationToken) =>
        unitOfWork.ExecuteInTransactionAsync(async token =>
        {
            var now = timeProvider.GetUtcNow();

            var transfer = await transfers.LockNextDueAsync(now, token);
            if (transfer is null)
                return null;

            transfer.StartProcessing(now);

            var parties = await partiesLocker.LockAsync(transfer.SourceAccountId, transfer.DestinationAccountId, token);
            await executor.ExecuteAsync(transfer, parties, now, token);

            await unitOfWork.SaveChangesAsync(token);

            return TransferResponse.From(transfer);
        }, cancellationToken);
}
