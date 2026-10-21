using Transfers.Domain.Limits;

namespace Transfers.Application.Accounts;

public sealed record HourlyLimitResponse(decimal MaxAmountPerHour, int MaxAttemptsPerHour)
{
    public static HourlyLimitResponse From(HourlyLimit limit) =>
        new(limit.MaxAmountPerHour, limit.MaxAttemptsPerHour);
}
