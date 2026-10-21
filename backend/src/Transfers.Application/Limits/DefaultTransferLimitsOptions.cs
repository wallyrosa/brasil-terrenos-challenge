using Transfers.Domain.Limits;

namespace Transfers.Application.Limits;

public sealed class DefaultTransferLimitsOptions
{
    public const string SectionName = "DefaultTransferLimits";

    public HourlyLimitOptions Daytime { get; init; } = new() { MaxAmountPerHour = 5000, MaxAttemptsPerHour = 5 };

    public HourlyLimitOptions Nighttime { get; init; } = new() { MaxAmountPerHour = 1000, MaxAttemptsPerHour = 3 };

    public TransferLimits ToTransferLimits() =>
        TransferLimits.Create(Daytime.ToHourlyLimit(), Nighttime.ToHourlyLimit());
}
