using Transfers.Domain.Limits;

namespace Transfers.Application.Limits;

public sealed class HourlyLimitOptions
{
    public decimal MaxAmountPerHour { get; init; }

    public int MaxAttemptsPerHour { get; init; }

    public HourlyLimit ToHourlyLimit() => HourlyLimit.Create(MaxAmountPerHour, MaxAttemptsPerHour);
}
