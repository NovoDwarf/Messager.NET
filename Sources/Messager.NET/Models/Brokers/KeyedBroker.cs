using Messager.NET.Brokers;
using Messager.NET.Entities;
using Messager.NET.Extensions;
using Messager.NET.Models.Resources;
using Messager.NET.Receivers;
using Messager.NET.Senders;
using Microsoft.Extensions.Logging;

namespace Messager.NET.Models.Brokers;

public partial class KeyedBroker<TKey, TEvent> : IKeyedBroker, ISender<TKey, TEvent>, IReceiver<TKey, TEvent>
    where TKey : notnull
{
    private readonly Dictionary<TKey, List<SubscriptionSync<TEvent>>> _handlers = new();
    private readonly Lock _locker = new();
    private readonly ILogger<KeyedBroker<TKey, TEvent>>? _logger;

    public KeyedBroker(ILogger<KeyedBroker<TKey, TEvent>>? logger = null)
    {
        _logger = logger;
    }

    public Guid Id { get; set; } = Guid.NewGuid();

    public string BrokerType => typeof(SimpleBroker<>).Name;
    public string KeyType => typeof(TKey).Name;
    public string EventType => typeof(TEvent).Name;

    public void Send(TKey key, TEvent evt)
    {
        List<SubscriptionSync<TEvent>> activeHandlers;

        lock (_locker)
        {
            if (!_handlers.TryGetValue(key, out var list))
            {
                _logger?.LogKeyNotFound(BrokerType, KeyType, EventType, Id, key.ToString() ?? "Unknown");
                return;
            }

            TryRemove(list);

            activeHandlers = [.. list];

            if (list.Count == 0)
            {
                _handlers.Remove(key);
                _logger?.LogRemovedEmptyKey(BrokerType, KeyType, EventType, Id, key.ToString() ?? "Unknown");
            }
        }

        foreach (var sub in activeHandlers)
            TryInvoke(sub, evt);
    }

    public IDisposable Subscribe(TKey key, Action<TEvent> handler)
    {
        var subscription = Subscribe(key, handler, SubscriptionReference.Weak);

        return new Unsubscriber(() =>
        {
            subscription.Dispose();
            GC.KeepAlive(handler); // Preserve the legacy handle's ownership of its callback targets.
        });
    }

    public IDisposable Subscribe(TKey key, Action<TEvent> handler, SubscriptionReference reference)
    {
        var subscription = new SubscriptionSync<TEvent>(handler, reference);

        lock (_locker)
        {
            if (!_handlers.TryGetValue(key, out var list))
            {
                list = [];
                _handlers[key] = list;
                _logger?.LogCreateSubscriptionListForKey(BrokerType, KeyType, EventType, Id, key.ToString() ?? "Unknown");
            }
            
            list.Add(subscription);
            _logger?.LogSubscriberAddedForKey(BrokerType, KeyType, EventType, Id, key.ToString() ?? "Unknown");
        }

        return new Unsubscriber(() =>
        {
            lock (_locker)
            {
                if (!_handlers.TryGetValue(key, out var list))
                    return;

                list.Remove(subscription);
                _logger?.LogSubscriberRemovedForKey(BrokerType, KeyType, EventType, Id, string.Empty);

                if (list.Count != 0)
                    return;

                _handlers.Remove(key);
                _logger?.LogRemovedEmptyKey(BrokerType, KeyType, EventType, Id, key.ToString() ?? "Unknown");
            }

        });
    }

    private void TryRemove(List<SubscriptionSync<TEvent>> list)
    {
        var removedCount = list.RemoveAll(s => !s.IsAlive);

        if (removedCount <= 0)
            return;

        lock (_locker)
        {
            _logger?.LogSubscribersRemovedForKey(BrokerType, KeyType, EventType, Id, string.Empty, removedCount);
        }
    }

    private void TryInvoke(SubscriptionSync<TEvent> sub, TEvent evt)
    {
        try
        {
            sub.Invoke(evt);
        }
        catch (Exception ex)
        {
            lock (_locker)
            {
                _logger?.LogErrorInvokingHandler(ex, BrokerType, EventType, Id);
            }
        }
    }
}
