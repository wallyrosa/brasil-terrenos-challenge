using Transfers.Application.Abstractions.Persistence;

namespace Transfers.Application.Accounts.ListAccounts;

public sealed class ListAccountsHandler(IAccountRepository accounts)
{
    public async Task<IReadOnlyList<AccountResponse>> HandleAsync(CancellationToken cancellationToken)
    {
        var result = await accounts.ListAsync(cancellationToken);

        return result.Select(AccountResponse.From).ToList();
    }
}
