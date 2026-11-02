using Messager.NET.Brokers;
using Messager.NET.Entities;
using Messager.NET.Extensions;
using Messager.NET.Models.Resources;
using Messager.NET.Receivers;
using Messager.NET.Senders;
using Microsoft.Extensions.Logging;

namespace Messager.NET.Models.Brokers;

public class AsyncKeyedBroker<TKey, TEvent> : IKeyedBroker, IAsyncSender<TKey, TEvent>, IAsyncReceiver<TKey, TEvent>
    where TKey : notnull
{
    private readonly Dictionary<TKey, List<SubscriptionAsync<TEvent>>> _handlers = new();
    private readonly Lock _locker = new();
    private readonly ILogger<AsyncKeyedBroker<TKey, TEvent>>? _logger;

    public AsyncKeyedBroker(ILogger<AsyncKeyedBroker<TKey, TEvent>>? logger = null)
    {
        _logger = logger;
    }

    public Guid Id { get; set; } = Guid.NewGuid();

    public string BrokerType => typeof(AsyncKeyedBroker<TKey, TEvent>).Name;
    public string KeyType => typeof(TKey).Name;
    public string EventType => typeof(TEvent).Name;

    public IAsyncDisposable Subscribe(TKey key, Func<TEvent, ValueTask> handler) => Subscribe(key, handler, SubscriptionReference.Strong);

    public IAsyncDisposable Subscribe(TKey key, Func<TEvent, ValueTask> handler, SubscriptionReference reference)
    {
        var subscription = new SubscriptionAsync<TEvent>(handler, reference);

        lock (_locker)
        {
            if (!_handlers.TryGetValue(key, out var list))
            {
                list = [];
                _handlers[key] = list;

            }
            list.Add(subscription);
            
            _logger?.LogSubscriberAddedForKeyKeyInAsynckeyedbrokerKeytypeEventtypeTotalSubscribersFor(key, typeof(TKey).Name, typeof(TEvent).Name, list.Count);
        }

        var subscriptionReference = new WeakReference<SubscriptionAsync<TEvent>>(subscription);

        return new UnsubscriberAsync(() =>
        {
            if (!subscriptionReference.TryGetTarget(out var registeredSubscription))
                return ValueTask.CompletedTask;
            
            lock (_locker)
            {
                if (!_handlers.TryGetValue(key, out var list))
                    return ValueTask.CompletedTask;
                
                list.Remove(registeredSubscription);

                if (list.Count == 0)
                    _handlers.Remove(key);
                
                return ValueTask.CompletedTask;
            }
        });
    }

    public void Unsubscribe(TKey key, Func<TEvent, ValueTask> handler)
    {
        lock (_locker)
        {
            if (!_handlers.TryGetValue(key, out var list))
                return;
            
            var index = list.FindIndex(subscription => subscription.Matches(handler));

            if (index >= 0)
                list.RemoveAt(index);
            
            if (list.Count == 0)
                _handlers.Remove(key);
        }
    }

    public async ValueTask SendAsync(TKey key, TEvent evt)
    {
        List<SubscriptionAsync<TEvent>>? handlersCopy = null;

        lock (_locker)
        {
            if (_handlers.TryGetValue(key, out var list))
            {
                list.RemoveAll(subscription => !subscription.IsAlive);
                handlersCopy = [.. list];

                if (list.Count == 0)
                    _handlers.Remove(key);
            }
        }

        if (handlersCopy == null || handlersCopy.Count == 0)
            return;

        foreach (var handler in handlersCopy)
            await TryInvokeAsync(handler, key, evt);
    }

    private async Task TryInvokeAsync(SubscriptionAsync<TEvent> handler, TKey key, TEvent evt)
    {
        try
        {
            await handler.InvokeAsync(evt).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error invoking async handler for event {EventType} with key {Key}", typeof(TEvent).Name, key);
        }
    }
}
