using Transfers.Domain.Accounts;

namespace Transfers.Application.Accounts;

public sealed record AccountResponse(
    Guid Id,
    string HolderName,
    string Document,
    AccountStatus Status,
    decimal Balance,
    decimal OverdraftLimit,
    decimal UsedOverdraft,
    decimal AvailableOverdraft,
    decimal AvailableToTransfer,
    TransferLimitsResponse TransferLimits,
    DateTimeOffset CreatedAt)
{
    public static AccountResponse From(Account account) =>
        new(
            account.Id,
            account.HolderName,
            account.Document,
            account.Status,
            account.Balance,
            account.OverdraftLimit,
            account.UsedOverdraft,
            account.AvailableOverdraft,
            account.AvailableToTransfer,
            TransferLimitsResponse.From(account.TransferLimits),
            account.CreatedAt);
}
