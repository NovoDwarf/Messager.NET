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
    private readonly List<Subscriber<TEvent>> _handlers = [];
    private readonly Lock _locker = new();

    private readonly ILogger<SimpleBroker<TEvent>> _logger;

    public SimpleBroker(
        ILogger<SimpleBroker<TEvent>>? logger = null)
    {
        _logger = logger ??
            NullLogger<SimpleBroker<TEvent>>.Instance;
    }

    public Guid Id { get; } = Guid.NewGuid();

    public string BrokerType => typeof(SimpleBroker<>).Name;
    public string EventType => typeof(TEvent).Name;

    public void Send(TEvent evt)
    {
        Subscriber<TEvent>[] handlers;
        
        var removedCount = 0;

        lock (_locker)
        {
            removedCount = _handlers.RemoveAll(subscription => !subscription.IsAlive);
            handlers = [.. _handlers];
        }

        if (removedCount > 0) 
            _logger.LogSubscribersRemoved(BrokerType, EventType, Id, removedCount);

        foreach (var handler in handlers)
            TryInvoke(handler, evt);
    }

    public IDisposable Subscribe(Action<TEvent> handler, SubscriptionReference reference = SubscriptionReference.Strong)
    {
        var subscription = new Subscriber<TEvent>(
            handler, reference);

        lock (_locker)
            _handlers.Add(subscription);

        _logger.LogSubscriberAdded(BrokerType, EventType, Id);

        return new Unsubscriber(() =>
        {
            var removed = false;

            lock (_locker)
                removed = _handlers.Remove(subscription);

            if (removed) 
                _logger.LogSubscriberRemoved(BrokerType, EventType, Id);
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
