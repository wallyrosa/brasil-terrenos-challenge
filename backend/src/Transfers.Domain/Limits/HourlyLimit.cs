using Transfers.Domain.Common;

namespace Transfers.Domain.Limits;

public sealed record HourlyLimit
{
    public decimal MaxAmountPerHour { get; private init; }

    public int MaxAttemptsPerHour { get; private init; }

    private HourlyLimit()
    {
    }

    public static HourlyLimit Create(decimal maxAmountPerHour, int maxAttemptsPerHour)
    {
        if (maxAmountPerHour <= 0 || maxAttemptsPerHour <= 0)
            throw DomainErrors.InvalidHourlyLimit();

        Money.EnsurePositive(maxAmountPerHour);

        return new HourlyLimit
        {
            MaxAmountPerHour = maxAmountPerHour,
            MaxAttemptsPerHour = maxAttemptsPerHour
        };
    }
}
