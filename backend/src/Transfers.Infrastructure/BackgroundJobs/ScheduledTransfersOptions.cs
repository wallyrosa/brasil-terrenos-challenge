namespace Transfers.Infrastructure.BackgroundJobs;

public sealed class ScheduledTransfersOptions
{
    public const string SectionName = "ScheduledTransfers";

    public bool Enabled { get; init; } = true;

    public TimeSpan PollingInterval { get; init; } = TimeSpan.FromSeconds(5);

    public int BatchSize { get; init; } = 50;
}
