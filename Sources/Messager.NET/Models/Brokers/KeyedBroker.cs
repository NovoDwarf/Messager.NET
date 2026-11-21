using Messager.NET.Brokers;
using Messager.NET.Entities;
using Messager.NET.Extensions;
using Messager.NET.Receivers;
using Messager.NET.Senders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Messager.NET.Models.Brokers;

public class KeyedBroker<TKey, TEvent> : IKeyedBroker, ISender<TKey, TEvent>, IReceiver<TKey, TEvent> 
    where TKey : notnull
{
    private readonly Dictionary<TKey, List<Subscriber<TEvent>>> _handlers = new();
    private readonly Lock _locker = new();
    
    private readonly ILogger<KeyedBroker<TKey, TEvent>> _logger;

    public KeyedBroker(ILogger<KeyedBroker<TKey, TEvent>>? logger = null)
    {
        _logger = logger ?? NullLogger<KeyedBroker<TKey, TEvent>>.Instance;
    }

    public Guid Id { get; } = Guid.NewGuid();
    
    public string BrokerType => typeof(SimpleBroker<>).Name;
    public string KeyType => typeof(TKey).Name;
    public string EventType => typeof(TEvent).Name;

    public void Send(TKey key, TEvent evt)
    {
        List<Subscriber<TEvent>> activeHandlers;
        
        var removedCount = 0;
        var removedKey = false;

        lock (_locker)
        {
            if (!_handlers.TryGetValue(key, out var list))
            {
                activeHandlers = [];
                removedCount = 0;
                removedKey = false;
            }
            else
            {
                removedCount = list.RemoveAll(s => !s.IsAlive);
                activeHandlers = [.. list];

                removedKey = list.Count == 0;

                if (removedKey)
                    _handlers.Remove(key);
            }
        }

        if (removedCount > 0) 
            _logger.LogKeyedSubscribersRemoved(BrokerType, KeyType, EventType, Id, key, removedCount);

        if (removedKey)
            _logger.LogKeyedSubscriptionListRemoved(BrokerType, KeyType, EventType, Id, key);

        foreach (var subscriber in activeHandlers)
            TryInvoke(subscriber, evt);
    }

    public IDisposable Subscribe(TKey key, Action<TEvent> handler, SubscriptionReference reference)
    {
        var subscription = new Subscriber<TEvent>(handler, reference);

        lock (_locker)
        {
            if (!_handlers.TryGetValue(key, out var list))
            {
                list = [];
                _handlers[key] = list;
                _logger.LogKeyedSubscriptionListCreated(BrokerType, KeyType, EventType, Id, key);
            }
            
            list.Add(subscription);
        }
        
        _logger.LogKeyedSubscriptionListCreated(BrokerType, KeyType, EventType, Id, key);
        
        return new Unsubscriber(() =>
        {
            lock (_locker)
            {
                if (!_handlers.TryGetValue(key, out var list))
                    return;

                list.Remove(subscription);
                _logger.LogKeyedSubscriptionListRemoved(BrokerType, KeyType, EventType, Id, string.Empty);

                if (list.Count != 0)
                    return;

                _handlers.Remove(key);
            }
            
            _logger.LogKeyNotFound(BrokerType, KeyType, EventType, Id, key);
        });
    }

    private void TryRemove(List<Subscriber<TEvent>> list)
    {
        var removedCount = list.RemoveAll(s => !s.IsAlive);

        if (removedCount <= 0)
            return;

        _logger.LogKeyedSubscribersRemoved(BrokerType, KeyType, EventType, Id, string.Empty, removedCount);
    }

    private void TryInvoke(Subscriber<TEvent> sub, TEvent evt)
    {
        try
        {
            sub.Invoke(evt);
        }
        catch (Exception ex)
        {
            _logger.LogHandlerFailed(ex, BrokerType, EventType, Id);
        }
    }
}
