using System.ComponentModel.DataAnnotations;
using Transfers.Domain.Limits;

namespace Transfers.Api.Contracts.Accounts;

public sealed record HourlyLimitRequest(
    [Range(0.01, double.MaxValue)] decimal MaxAmountPerHour,
    [Range(1, int.MaxValue)] int MaxAttemptsPerHour)
{
    public HourlyLimit ToHourlyLimit() => HourlyLimit.Create(MaxAmountPerHour, MaxAttemptsPerHour);
}
