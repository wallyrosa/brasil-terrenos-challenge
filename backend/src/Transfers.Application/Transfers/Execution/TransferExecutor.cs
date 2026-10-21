using Transfers.Application.Abstractions.Persistence;
using Transfers.Application.Limits;
using Transfers.Domain.Transfers;

namespace Transfers.Application.Transfers.Execution;

public sealed class TransferExecutor(
    ITransferRepository transfers,
    ITransferPeriodResolver periodResolver,
    TransferExecutionService executionService)
{
    private static readonly TimeSpan UsageWindow = TimeSpan.FromHours(1);

    public async Task ExecuteAsync(
        Transfer transfer,
        TransferParties parties,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var usage = await transfers.GetHourlyUsageAsync(parties.Source.Id, now - UsageWindow, cancellationToken);
        var period = periodResolver.Resolve(now);

        executionService.Execute(transfer, parties.Source, parties.Destination, period, usage, now);
    }
}
