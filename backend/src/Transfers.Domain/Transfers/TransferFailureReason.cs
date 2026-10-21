namespace Transfers.Domain.Transfers;

public enum TransferFailureReason
{
    SourceAccountInactive = 1,
    DestinationAccountInactive = 2,
    InsufficientFunds = 3,
    HourlyAmountLimitExceeded = 4,
    HourlyAttemptsLimitExceeded = 5
}
