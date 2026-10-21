using Transfers.Domain.Accounts;
using Transfers.Domain.Common;
using Transfers.Domain.Limits;

namespace Transfers.Domain.Transfers;

public sealed class TransferExecutionService
{
    public void Execute(
        Transfer transfer,
        Account source,
        Account destination,
        TransferPeriod period,
        HourlyUsage usage,
        DateTimeOffset now)
    {
        if (transfer.SourceAccountId != source.Id || transfer.DestinationAccountId != destination.Id)
            throw DomainErrors.TransferAccountsMismatch();

        var failureReason = Evaluate(transfer, source, destination, source.TransferLimits.For(period), usage);

        if (failureReason is not null)
        {
            transfer.Fail(failureReason.Value, now);
            return;
        }

        source.Debit(transfer.Amount);
        destination.Credit(transfer.Amount);
        transfer.Complete(now);
    }

    private static TransferFailureReason? Evaluate(
        Transfer transfer,
        Account source,
        Account destination,
        HourlyLimit limit,
        HourlyUsage usage)
    {
        if (!source.IsActive)
            return TransferFailureReason.SourceAccountInactive;

        if (!destination.IsActive)
            return TransferFailureReason.DestinationAccountInactive;

        if (usage.Attempts >= limit.MaxAttemptsPerHour)
            return TransferFailureReason.HourlyAttemptsLimitExceeded;

        if (usage.TransferredAmount + transfer.Amount > limit.MaxAmountPerHour)
            return TransferFailureReason.HourlyAmountLimitExceeded;

        if (!source.CanAfford(transfer.Amount))
            return TransferFailureReason.InsufficientFunds;

        return null;
    }
}
