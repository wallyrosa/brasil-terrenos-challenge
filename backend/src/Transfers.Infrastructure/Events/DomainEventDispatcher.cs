using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Transfers.Application.Abstractions.Events;
using Transfers.Domain.Common;

namespace Transfers.Infrastructure.Events;

internal sealed class DomainEventDispatcher(
    IServiceProvider serviceProvider,
    ILogger<DomainEventDispatcher> logger) : IDomainEventDispatcher
{
    private static readonly MethodInfo DispatchEventMethod = typeof(DomainEventDispatcher)
        .GetMethod(nameof(DispatchEventAsync), BindingFlags.Instance | BindingFlags.NonPublic)!;

    public async Task DispatchAsync(IReadOnlyCollection<IDomainEvent> domainEvents, CancellationToken cancellationToken)
    {
        foreach (var domainEvent in domainEvents)
        {
            try
            {
                var dispatch = DispatchEventMethod.MakeGenericMethod(domainEvent.GetType());
                await (Task)dispatch.Invoke(this, [domainEvent, cancellationToken])!;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Failed to handle domain event {EventType}", domainEvent.GetType().Name);
            }
        }
    }

    private async Task DispatchEventAsync<TEvent>(TEvent domainEvent, CancellationToken cancellationToken)
        where TEvent : IDomainEvent
    {
        foreach (var handler in serviceProvider.GetServices<IDomainEventHandler<TEvent>>())
            await handler.HandleAsync(domainEvent, cancellationToken);
    }
}
