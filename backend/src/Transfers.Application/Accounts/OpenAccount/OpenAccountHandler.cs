using Microsoft.Extensions.Options;
using Transfers.Application.Abstractions.Persistence;
using Transfers.Application.Limits;
using Transfers.Domain.Accounts;

namespace Transfers.Application.Accounts.OpenAccount;

public sealed class OpenAccountHandler(
    IAccountRepository accounts,
    IUnitOfWork unitOfWork,
    IOptions<DefaultTransferLimitsOptions> defaultLimits,
    TimeProvider timeProvider)
{
    public async Task<AccountResponse> HandleAsync(OpenAccountCommand command, CancellationToken cancellationToken)
    {
        var account = Account.Open(
            command.HolderName,
            command.Document,
            command.InitialBalance,
            command.OverdraftLimit,
            defaultLimits.Value.ToTransferLimits(),
            timeProvider.GetUtcNow());

        accounts.Add(account);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return AccountResponse.From(account);
    }
}
