namespace Transfers.Application.Limits;

public sealed class TransferPeriodOptions
{
    public const string SectionName = "TransferPeriods";

    public string TimeZoneId { get; init; } = "America/Sao_Paulo";

    public TimeOnly DayStartsAt { get; init; } = new(6, 0);

    public TimeOnly NightStartsAt { get; init; } = new(22, 0);
}
