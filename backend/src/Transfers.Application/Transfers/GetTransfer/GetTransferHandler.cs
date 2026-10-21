using Transfers.Application.Abstractions.Persistence;
using Transfers.Application.Common.Exceptions;

namespace Transfers.Application.Transfers.GetTransfer;

public sealed class GetTransferHandler(ITransferRepository transfers)
{
    public async Task<TransferResponse> HandleAsync(Guid transferId, CancellationToken cancellationToken)
    {
        var transfer = await transfers.GetByIdAsync(transferId, cancellationToken)
            ?? throw ApplicationErrors.TransferNotFound(transferId);

        return TransferResponse.From(transfer);
    }
}
