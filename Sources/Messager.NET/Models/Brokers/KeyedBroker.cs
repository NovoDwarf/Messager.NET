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
    private readonly Dictionary<TKey, Subscriber<TEvent>[]> _handlers = new();
    private readonly Lock _locker = new();

    private readonly ILogger<KeyedBroker<TKey, TEvent>> _logger;

    public KeyedBroker(ILogger<KeyedBroker<TKey, TEvent>>? logger = null)
    {
        _logger = logger ?? NullLogger<KeyedBroker<TKey, TEvent>>.Instance;
    }

    public Guid Id { get; } = Guid.NewGuid();

    public string BrokerType => typeof(KeyedBroker<,>).Name;
    public string KeyType => typeof(TKey).Name;
    public string EventType => typeof(TEvent).Name;

    public void Send(TKey key, TEvent evt)
    {
        Subscriber<TEvent>[] handlers;

        lock (_locker)
        {
            if (!_handlers.TryGetValue(key, out handlers!))
                return;
        }

        var hasDeadSubscriptions = false;
        foreach (var subscriber in handlers)
        {
            hasDeadSubscriptions |= !TryInvoke(subscriber, evt);
        }

        if (hasDeadSubscriptions)
        {
            lock (_locker)
            {
                if (_handlers.TryGetValue(key, out var current))
                {
                    var live = Array.FindAll(current, static subscriber => subscriber.IsAlive);
                    if (live.Length == 0)
                        _handlers.Remove(key);
                    else if (live.Length != current.Length)
                        _handlers[key] = live;
                }
            }
        }
    }

    public IDisposable Subscribe(TKey key, Action<TEvent> handler, SubscriptionReference reference)
    {
        var subscription = new Subscriber<TEvent>(handler, reference);
        var createdKey = false;

        lock (_locker)
        {
            createdKey = !_handlers.TryGetValue(key, out var current);

            current ??= [];

            var updated = new Subscriber<TEvent>[current.Length + 1];

            Array.Copy(current, updated, current.Length);
            updated[^1] = subscription;

            _handlers[key] = updated;
        }

        if (createdKey)
            _logger.LogKeyedSubscriptionListCreated(BrokerType, KeyType, EventType, Id, key);

        _logger.LogKeyedSubscriberAdded(BrokerType, KeyType, EventType, Id, key, 1);

        return new Unsubscriber(() =>
        {
            var removed = false;
            var removedKey = false;

            lock (_locker)
            {
                if (_handlers.TryGetValue(key, out var current))
                {
                    var index = Array.IndexOf(current, subscription);

                    if (index >= 0)
                    {
                        removed = true;

                        if (current.Length == 1)
                        {
                            _handlers.Remove(key);
                            removedKey = true;
                        }
                        else
                        {
                            var updated = new Subscriber<TEvent>[current.Length - 1];

                            if (index > 0)
                                Array.Copy(current, 0, updated, 0, index);

                            if (index < updated.Length)
                                Array.Copy(current, index + 1, updated, index, updated.Length - index);

                            _handlers[key] = updated;
                        }
                    }
                }
            }

            if (removed)
                _logger.LogKeyedSubscriberRemoved(BrokerType, KeyType, EventType, Id, key, 1);

            if (removedKey)
                _logger.LogKeyedSubscriptionListRemoved(BrokerType, KeyType, EventType, Id, key);
        });
    }

    private bool TryInvoke(Subscriber<TEvent> subscriber, TEvent evt)
    {
        try
        {
            return subscriber.Invoke(evt);
        }
        catch (Exception ex)
        {
            _logger.LogHandlerFailed(ex, BrokerType, EventType, Id);
            return true;
        }
    }
}