using Transfers.Application.Abstractions.Persistence;
using Transfers.Application.Common.Exceptions;

namespace Transfers.Application.Transfers.Execution;

public sealed class TransferPartiesLocker(IAccountRepository accounts)
{
    public async Task<TransferParties> LockAsync(
        Guid sourceAccountId,
        Guid destinationAccountId,
        CancellationToken cancellationToken)
    {
        var locked = await accounts.LockAsync([sourceAccountId, destinationAccountId], cancellationToken);

        var source = locked.SingleOrDefault(account => account.Id == sourceAccountId)
            ?? throw ApplicationErrors.AccountNotFound(sourceAccountId);

        var destination = locked.SingleOrDefault(account => account.Id == destinationAccountId)
            ?? throw ApplicationErrors.AccountNotFound(destinationAccountId);

        return new TransferParties(source, destination);
    }
}
