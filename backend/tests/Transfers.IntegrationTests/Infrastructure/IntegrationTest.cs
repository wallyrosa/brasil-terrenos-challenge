using Microsoft.Extensions.DependencyInjection;
using Transfers.Application.Transfers.ProcessScheduledTransfer;

namespace Transfers.IntegrationTests.Infrastructure;

[Collection(ApiCollection.Name)]
public abstract class IntegrationTest
{
    private static readonly TimeSpan BrasiliaOffset = TimeSpan.FromHours(-3);

    protected IntegrationTest(TransfersApiFactory factory)
    {
        Factory = factory;
        Api = new TransfersApiClient(factory.CreateClient());
        MoveToNextBrasiliaTime(new TimeOnly(12, 0));
    }

    protected TransfersApiFactory Factory { get; }

    protected TransfersApiClient Api { get; }

    protected DateTimeOffset Now => Factory.Clock.GetUtcNow();

    protected void MoveToNextBrasiliaTime(TimeOnly time)
    {
        var local = Now.ToOffset(BrasiliaOffset);
        var target = new DateTimeOffset(DateOnly.FromDateTime(local.DateTime).ToDateTime(time), BrasiliaOffset);

        if (target <= local)
            target = target.AddDays(1);

        Factory.Clock.SetUtcNow(target.ToUniversalTime());
    }

    protected void Advance(TimeSpan delta) => Factory.Clock.Advance(delta);

    protected async Task ProcessDueScheduledTransfersAsync()
    {
        while (true)
        {
            await using var scope = Factory.Services.CreateAsyncScope();
            var handler = scope.ServiceProvider.GetRequiredService<ProcessNextScheduledTransferHandler>();

            if (await handler.HandleAsync(CancellationToken.None) is null)
                return;
        }
    }
}
