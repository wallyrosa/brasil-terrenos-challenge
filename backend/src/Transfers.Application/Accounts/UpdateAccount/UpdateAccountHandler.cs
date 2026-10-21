using Transfers.Application.Abstractions.Persistence;
using Transfers.Application.Common.Exceptions;
using Transfers.Domain.Accounts;
using Transfers.Domain.Limits;

namespace Transfers.Application.Accounts.UpdateAccount;

public sealed class UpdateAccountHandler(IAccountRepository accounts, IUnitOfWork unitOfWork)
{
    public Task<AccountResponse> ChangeOverdraftLimitAsync(
        Guid accountId,
        decimal overdraftLimit,
        CancellationToken cancellationToken) =>
        UpdateAsync(accountId, account => account.ChangeOverdraftLimit(overdraftLimit), cancellationToken);

    public Task<AccountResponse> ChangeTransferLimitsAsync(
        Guid accountId,
        TransferLimits transferLimits,
        CancellationToken cancellationToken) =>
        UpdateAsync(accountId, account => account.ChangeTransferLimits(transferLimits), cancellationToken);

    public Task<AccountResponse> ChangeStatusAsync(
        Guid accountId,
        AccountStatusAction action,
        CancellationToken cancellationToken) =>
        UpdateAsync(accountId, account => Apply(account, action), cancellationToken);

    private async Task<AccountResponse> UpdateAsync(
        Guid accountId,
        Action<Account> change,
        CancellationToken cancellationToken) =>
        await unitOfWork.ExecuteInTransactionAsync(async token =>
        {
            var locked = await accounts.LockAsync([accountId], token);
            var account = locked.SingleOrDefault() ?? throw ApplicationErrors.AccountNotFound(accountId);

            change(account);
            await unitOfWork.SaveChangesAsync(token);

            return AccountResponse.From(account);
        }, cancellationToken);

    private static void Apply(Account account, AccountStatusAction action)
    {
        switch (action)
        {
            case AccountStatusAction.Activate:
                account.Activate();
                break;
            case AccountStatusAction.Block:
                account.Block();
                break;
            case AccountStatusAction.Deactivate:
                account.Deactivate();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(action), action, null);
        }
    }
}
