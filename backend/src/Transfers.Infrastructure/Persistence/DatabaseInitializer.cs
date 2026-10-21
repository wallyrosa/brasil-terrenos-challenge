using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Transfers.Application.Limits;
using Transfers.Domain.Accounts;

namespace Transfers.Infrastructure.Persistence;

internal sealed class DatabaseInitializer(
    IServiceScopeFactory scopeFactory,
    IOptions<DatabaseOptions> options,
    IOptions<DefaultTransferLimitsOptions> defaultLimits,
    TimeProvider timeProvider) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<TransfersDbContext>();

        if (options.Value.ApplyMigrationsOnStartup)
            await context.Database.MigrateAsync(cancellationToken);

        if (options.Value.SeedDemoData)
            await SeedAsync(context, cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private async Task SeedAsync(TransfersDbContext context, CancellationToken cancellationToken)
    {
        if (await context.Accounts.AnyAsync(cancellationToken))
            return;

        var limits = defaultLimits.Value.ToTransferLimits();
        var now = timeProvider.GetUtcNow();

        context.Accounts.AddRange(
            Account.Open("Ana Souza", "12345678901", 500m, 1000m, limits, now),
            Account.Open("Bruno Lima", "23456789012", 2000m, 500m, limits, now),
            Account.Open("Carla Mendes", "34567890123", 10000m, 0m, limits, now));

        await context.SaveChangesAsync(cancellationToken);
    }
}
