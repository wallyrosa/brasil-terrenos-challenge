using System.ComponentModel.DataAnnotations;
using Transfers.Domain.Limits;

namespace Transfers.Api.Contracts.Accounts;

public sealed record ChangeTransferLimitsRequest(
    [Required] HourlyLimitRequest Daytime,
    [Required] HourlyLimitRequest Nighttime)
{
    public TransferLimits ToTransferLimits() =>
        TransferLimits.Create(Daytime.ToHourlyLimit(), Nighttime.ToHourlyLimit());
}
