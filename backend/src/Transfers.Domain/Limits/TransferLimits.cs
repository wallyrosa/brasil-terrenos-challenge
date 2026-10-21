namespace Transfers.Domain.Limits;

public sealed record TransferLimits
{
    public HourlyLimit Daytime { get; private init; } = null!;

    public HourlyLimit Nighttime { get; private init; } = null!;

    private TransferLimits()
    {
    }

    public static TransferLimits Create(HourlyLimit daytime, HourlyLimit nighttime) =>
        new()
        {
            Daytime = daytime,
            Nighttime = nighttime
        };

    public HourlyLimit For(TransferPeriod period) =>
        period == TransferPeriod.Day ? Daytime : Nighttime;
}
