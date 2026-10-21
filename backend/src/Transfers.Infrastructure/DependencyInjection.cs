using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Transfers.Application.Abstractions.Events;
using Transfers.Application.Abstractions.Persistence;
using Transfers.Infrastructure.BackgroundJobs;
using Transfers.Infrastructure.Events;
using Transfers.Infrastructure.Persistence;
using Transfers.Infrastructure.Persistence.Repositories;

namespace Transfers.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Transfers")
            ?? throw new InvalidOperationException("Connection string 'Transfers' was not configured.");

        services.AddDbContext<TransfersDbContext>(options => options
            .UseNpgsql(connectionString)
            .UseSnakeCaseNamingConvention());

        services.Configure<DatabaseOptions>(configuration.GetSection(DatabaseOptions.SectionName));
        services.Configure<ScheduledTransfersOptions>(configuration.GetSection(ScheduledTransfersOptions.SectionName));

        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<ITransferRepository, TransferRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();

        services.AddHostedService<DatabaseInitializer>();

        var scheduledTransfers = configuration.GetSection(ScheduledTransfersOptions.SectionName).Get<ScheduledTransfersOptions>()
            ?? new ScheduledTransfersOptions();

        if (scheduledTransfers.Enabled)
            services.AddHostedService<ScheduledTransfersWorker>();

        return services;
    }
}
