namespace Transfers.Domain.Limits;

public sealed record HourlyUsage(decimal TransferredAmount, int Attempts)
{
    public static HourlyUsage None { get; } = new(0, 0);
}
