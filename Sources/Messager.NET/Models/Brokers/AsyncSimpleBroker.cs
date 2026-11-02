using Messager.NET.Brokers;
using Messager.NET.Entities;
using Messager.NET.Extensions;
using Messager.NET.Models.Resources;
using Messager.NET.Receivers;
using Messager.NET.Senders;
using Microsoft.Extensions.Logging;

namespace Messager.NET.Models.Brokers;

public class AsyncSimpleBroker<TEvent> : ISimpleBroker, IAsyncSender<TEvent>, IAsyncReceiver<TEvent>
{
    private readonly List<SubscriptionAsync<TEvent>> _handlers = [];
    private readonly Lock _locker = new();
    private readonly ILogger<AsyncSimpleBroker<TEvent>>? _logger;

    public AsyncSimpleBroker(ILogger<AsyncSimpleBroker<TEvent>>? logger = null)
    {
        _logger = logger;
    }

    public Guid Id { get; set; } = Guid.NewGuid();

    public string BrokerType => typeof(SimpleBroker<>).Name;
    public string EventType => typeof(TEvent).Name;

    public async ValueTask SendAsync(TEvent evt)
    {
        List<SubscriptionAsync<TEvent>> copy;

        lock (_locker)
        {
            _handlers.RemoveAll(subscription => !subscription.IsAlive);
            copy = [.. _handlers];
        }

        foreach (var handler in copy)
            await TryInvokeAsync(handler, evt);
    }

    public IAsyncDisposable Subscribe(Func<TEvent, ValueTask> handler) => Subscribe(handler, SubscriptionReference.Strong);

    public IAsyncDisposable Subscribe(Func<TEvent, ValueTask> handler, SubscriptionReference reference)
    {
        var subscription = new SubscriptionAsync<TEvent>(handler, reference);

        lock (_locker)
        {
            _handlers.Add(subscription);
            _logger?.LogSubscriberAdded(BrokerType, EventType, Id);
        }

        var subscriptionReference = new WeakReference<SubscriptionAsync<TEvent>>(subscription);

        return new UnsubscriberAsync(() =>
        {
            if (!subscriptionReference.TryGetTarget(out var registeredSubscription))
                return ValueTask.CompletedTask;

            lock (_locker)
            {
                _handlers.Remove(registeredSubscription);
                _logger?.LogSubscriberRemoved(BrokerType, EventType, Id);

                return ValueTask.CompletedTask;
            }
        });
    }

    public void Unsubscribe(Func<TEvent, ValueTask> handler)
    {
        lock (_locker)
        {
            var index = _handlers.FindIndex(subscription => subscription.Matches(handler));

            if (index < 0) 
                return;
            
            _handlers.RemoveAt(index);
            _logger?.LogSubscriberRemoved(BrokerType, EventType, Id);
        }
    }

    private async Task TryInvokeAsync(SubscriptionAsync<TEvent> handler, TEvent evt)
    {
        try
        {
            await handler.InvokeAsync(evt).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger?.LogErrorInvokingHandler(ex, BrokerType, EventType, Id);
        }
    }
}
