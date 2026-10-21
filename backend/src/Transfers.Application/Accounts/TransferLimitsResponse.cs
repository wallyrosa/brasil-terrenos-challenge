using Transfers.Domain.Limits;

namespace Transfers.Application.Accounts;

public sealed record TransferLimitsResponse(HourlyLimitResponse Daytime, HourlyLimitResponse Nighttime)
{
    public static TransferLimitsResponse From(TransferLimits limits) =>
        new(HourlyLimitResponse.From(limits.Daytime), HourlyLimitResponse.From(limits.Nighttime));
}
