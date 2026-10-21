using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Transfers.Application.Transfers.ProcessScheduledTransfer;

namespace Transfers.Infrastructure.BackgroundJobs;

internal sealed class ScheduledTransfersWorker(
    IServiceScopeFactory scopeFactory,
    IOptions<ScheduledTransfersOptions> options,
    ILogger<ScheduledTransfersWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(options.Value.PollingInterval);

        try
        {
            do
            {
                await ProcessDueTransfersAsync(stoppingToken);
            }
            while (await timer.WaitForNextTickAsync(stoppingToken));
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
        }
    }

    private async Task ProcessDueTransfersAsync(CancellationToken stoppingToken)
    {
        for (var processed = 0; processed < options.Value.BatchSize; processed++)
        {
            try
            {
                await using var scope = scopeFactory.CreateAsyncScope();
                var handler = scope.ServiceProvider.GetRequiredService<ProcessNextScheduledTransferHandler>();

                var transfer = await handler.HandleAsync(stoppingToken);
                if (transfer is null)
                    return;

                logger.LogInformation(
                    "Scheduled transfer {TransferId} processed with status {Status}",
                    transfer.Id,
                    transfer.Status);
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                logger.LogError(exception, "Failed to process scheduled transfers");
                return;
            }
        }
    }
}
