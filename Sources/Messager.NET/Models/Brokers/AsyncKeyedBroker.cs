using Messager.NET.Brokers;
using Messager.NET.Entities;
using Messager.NET.Extensions;
using Messager.NET.Receivers;
using Messager.NET.Senders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Messager.NET.Models.Brokers;

public class AsyncKeyedBroker<TKey, TEvent> : IKeyedBroker, IAsyncSender<TKey, TEvent>, IAsyncReceiver<TKey, TEvent>
    where TKey : notnull
{
    private readonly Dictionary<TKey, List<SubscriberAsync<TEvent>>> _handlers = new();
    private readonly Lock _locker = new();
    
    private readonly ILogger<AsyncKeyedBroker<TKey, TEvent>> _logger;

    public AsyncKeyedBroker(ILogger<AsyncKeyedBroker<TKey, TEvent>>? logger = null)
    {
        _logger = logger ?? NullLogger<AsyncKeyedBroker<TKey, TEvent>>.Instance;
    }

    public Guid Id { get; } = Guid.NewGuid();
    
    public string BrokerType => typeof(AsyncKeyedBroker<TKey, TEvent>).Name;
    public string KeyType => typeof(TKey).Name;
    public string EventType => typeof(TEvent).Name;
    
    public IAsyncDisposable Subscribe(TKey key, Func<TEvent, ValueTask> handler, SubscriptionReference reference)
    {
        var subscription = new SubscriberAsync<TEvent>(handler, reference);
        var created = false;
        var count = 0;

        lock (_locker)
        {
            created = !_handlers.TryGetValue(key, out var list);

            if (created)
            {
                list = [];
                _handlers[key] = list;
            }

            list!.Add(subscription);
            count = list.Count;
        }

        if (created) 
            _logger.LogKeyedSubscriptionListCreated(BrokerType, KeyType, EventType, Id, key);

        _logger.LogKeyedSubscriberAdded(BrokerType, KeyType, EventType, Id, key, count);

        return new UnsubscriberAsync(() =>
        {
            var removed = false;
            var removedKey = false;
            var remainingCount = 0;

            lock (_locker)
            {
                if (!_handlers.TryGetValue(key, out var list))
                    return ValueTask.CompletedTask;

                removed = list.Remove(subscription);
                remainingCount = list.Count;
                removedKey = removed && remainingCount == 0;

                if (removedKey)
                    _handlers.Remove(key);
            }

            if (removed) 
                _logger.LogKeyedSubscriberRemoved(BrokerType, KeyType, EventType, Id, key, remainingCount);

            if (removedKey) 
                _logger.LogKeyedSubscriptionListRemoved(BrokerType, KeyType, EventType, Id, key);

            return ValueTask.CompletedTask;
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
        List<SubscriberAsync<TEvent>>? handlersCopy = null;

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

    private async ValueTask TryInvokeAsync(SubscriberAsync<TEvent> handler, TKey key, TEvent evt)
    {
        try
        {
            await handler.InvokeAsync(evt).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invoking async handler for event {EventType} with key {Key}", typeof(TEvent).Name, key);
        }
    }
}