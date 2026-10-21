using Transfers.Application.Abstractions.Persistence;
using Transfers.Application.Common.Exceptions;

namespace Transfers.Application.Transfers.ListTransfers;

public sealed class ListTransfersHandler(IAccountRepository accounts, ITransferRepository transfers)
{
    public async Task<IReadOnlyList<TransferResponse>> HandleAsync(Guid accountId, CancellationToken cancellationToken)
    {
        _ = await accounts.GetByIdAsync(accountId, cancellationToken)
            ?? throw ApplicationErrors.AccountNotFound(accountId);

        var result = await transfers.ListByAccountAsync(accountId, cancellationToken);

        return result.Select(TransferResponse.From).ToList();
    }
}
