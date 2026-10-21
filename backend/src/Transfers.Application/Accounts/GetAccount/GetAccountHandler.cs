using Transfers.Application.Abstractions.Persistence;
using Transfers.Application.Common.Exceptions;

namespace Transfers.Application.Accounts.GetAccount;

public sealed class GetAccountHandler(IAccountRepository accounts)
{
    public async Task<AccountResponse> HandleAsync(Guid accountId, CancellationToken cancellationToken)
    {
        var account = await accounts.GetByIdAsync(accountId, cancellationToken)
            ?? throw ApplicationErrors.AccountNotFound(accountId);

        return AccountResponse.From(account);
    }
}
