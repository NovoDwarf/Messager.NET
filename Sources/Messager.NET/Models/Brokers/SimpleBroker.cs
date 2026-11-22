using Messager.NET.Brokers;
using Messager.NET.Entities;
using Messager.NET.Extensions;
using Messager.NET.Receivers;
using Messager.NET.Senders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Messager.NET.Models.Brokers;

public class SimpleBroker<TEvent> : ISimpleBroker, ISender<TEvent>, IReceiver<TEvent>
{
    private readonly Lock _locker = new();

    private Subscriber<TEvent>[] _handlers = [];
    
    private readonly ILogger<SimpleBroker<TEvent>> _logger;

    public SimpleBroker(ILogger<SimpleBroker<TEvent>>? logger = null)
    {
        _logger = logger ?? NullLogger<SimpleBroker<TEvent>>.Instance;
    }

    public Guid Id { get; } = Guid.NewGuid();

    public string BrokerType => typeof(SimpleBroker<>).Name;
    public string EventType => typeof(TEvent).Name;

    public void Send(TEvent evt)
    {
        var handlers = Volatile.Read(ref _handlers);

        foreach (var handler in handlers)
            TryInvoke(handler, evt);
    }

    public IDisposable Subscribe(Action<TEvent> handler, SubscriptionReference reference = SubscriptionReference.Strong)
    {
        var subscription = new Subscriber<TEvent>(
            handler, reference);

        lock (_locker)
        {
            var current = _handlers;
            var updated = new Subscriber<TEvent>[current.Length + 1];

            Array.Copy(current, updated, current.Length);
            updated[^1] = subscription;

            Volatile.Write(ref _handlers, updated);
        }

        _logger.LogSubscriberAdded(BrokerType, EventType, Id);

        return new Unsubscriber(() =>
        {
            bool removed;

            lock (_locker)
            {
                var current = _handlers;
                var index = Array.IndexOf(current, subscription);

                removed = index >= 0;

                if (removed)
                {
                    var updated = new Subscriber<TEvent>[current.Length - 1];

                    if (index > 0) 
                        Array.Copy(current, 0, updated, 0, index);

                    if (index < updated.Length) 
                        Array.Copy(current, index + 1, updated, index, updated.Length - index);

                    Volatile.Write(ref _handlers, updated);
                }
            }

            if (removed)
            {
                _logger.LogSubscriberRemoved(BrokerType, EventType, Id);
            }
        });
    }

    private void TryInvoke(Subscriber<TEvent> subscription, TEvent evt)
    {
        try
        {
            subscription.Invoke(evt);
        }
        catch (Exception exception)
        {
            _logger.LogHandlerFailed(exception, BrokerType, EventType, Id);
        }
    }
}
