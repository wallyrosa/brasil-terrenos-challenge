using Transfers.Application.Abstractions.Persistence;
using Transfers.Application.Common.Exceptions;

namespace Transfers.Application.Transfers.CancelTransfer;

public sealed class CancelTransferHandler(
    ITransferRepository transfers,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
{
    public Task<TransferResponse> HandleAsync(Guid transferId, CancellationToken cancellationToken) =>
        unitOfWork.ExecuteInTransactionAsync(async token =>
        {
            var transfer = await transfers.LockAsync(transferId, token)
                ?? throw ApplicationErrors.TransferNotFound(transferId);

            transfer.Cancel(timeProvider.GetUtcNow());
            await unitOfWork.SaveChangesAsync(token);

            return TransferResponse.From(transfer);
        }, cancellationToken);
}
