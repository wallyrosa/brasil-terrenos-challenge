using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Transfers.Application.Abstractions.Events;
using Transfers.Application.Accounts.GetAccount;
using Transfers.Application.Accounts.ListAccounts;
using Transfers.Application.Accounts.OpenAccount;
using Transfers.Application.Accounts.UpdateAccount;
using Transfers.Application.Limits;
using Transfers.Application.Transfers.CancelTransfer;
using Transfers.Application.Transfers.CreateTransfer;
using Transfers.Application.Transfers.EventHandlers;
using Transfers.Application.Transfers.Execution;
using Transfers.Application.Transfers.GetTransfer;
using Transfers.Application.Transfers.ListTransfers;
using Transfers.Application.Transfers.ProcessScheduledTransfer;
using Transfers.Application.Transfers.ScheduleTransfer;
using Transfers.Domain.Transfers;
using Transfers.Domain.Transfers.Events;

namespace Transfers.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<TransferPeriodOptions>(configuration.GetSection(TransferPeriodOptions.SectionName));
        services.Configure<DefaultTransferLimitsOptions>(configuration.GetSection(DefaultTransferLimitsOptions.SectionName));

        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<ITransferPeriodResolver, TransferPeriodResolver>();
        services.AddSingleton<TransferExecutionService>();

        services.AddScoped<TransferPartiesLocker>();
        services.AddScoped<IdempotencyGuard>();
        services.AddScoped<TransferExecutor>();

        services.AddScoped<OpenAccountHandler>();
        services.AddScoped<GetAccountHandler>();
        services.AddScoped<ListAccountsHandler>();
        services.AddScoped<UpdateAccountHandler>();

        services.AddScoped<CreateTransferHandler>();
        services.AddScoped<ScheduleTransferHandler>();
        services.AddScoped<CancelTransferHandler>();
        services.AddScoped<GetTransferHandler>();
        services.AddScoped<ListTransfersHandler>();
        services.AddScoped<ProcessNextScheduledTransferHandler>();

        services.AddScoped<IDomainEventHandler<TransferScheduled>, TransferAuditLogHandler>();
        services.AddScoped<IDomainEventHandler<TransferCompleted>, TransferAuditLogHandler>();
        services.AddScoped<IDomainEventHandler<TransferFailed>, TransferAuditLogHandler>();
        services.AddScoped<IDomainEventHandler<TransferCancelled>, TransferAuditLogHandler>();

        return services;
    }
}
