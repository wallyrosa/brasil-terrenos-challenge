using Microsoft.Extensions.Options;
using Transfers.Domain.Limits;

namespace Transfers.Application.Limits;

public sealed class TransferPeriodResolver : ITransferPeriodResolver
{
    private readonly TimeZoneInfo _timeZone;
    private readonly TimeOnly _dayStartsAt;
    private readonly TimeOnly _nightStartsAt;

    public TransferPeriodResolver(IOptions<TransferPeriodOptions> options)
    {
        _timeZone = TimeZoneInfo.FindSystemTimeZoneById(options.Value.TimeZoneId);
        _dayStartsAt = options.Value.DayStartsAt;
        _nightStartsAt = options.Value.NightStartsAt;
    }

    public TransferPeriod Resolve(DateTimeOffset instant)
    {
        var localTime = TimeOnly.FromTimeSpan(TimeZoneInfo.ConvertTime(instant, _timeZone).TimeOfDay);

        return localTime.IsBetween(_dayStartsAt, _nightStartsAt)
            ? TransferPeriod.Day
            : TransferPeriod.Night;
    }
}
