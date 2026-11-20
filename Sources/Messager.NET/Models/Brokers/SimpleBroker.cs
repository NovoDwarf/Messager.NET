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

    public SimpleBroker(ILogger<SimpleBroker<TEvent>>? logger = null)
    {
        _logger = logger ?? NullLogger<SimpleBroker<TEvent>>.Instance;
    }

    public Guid Id { get; set; } = Guid.NewGuid();
    public string BrokerType => typeof(SimpleBroker<>).Name;
    public string EventType => typeof(TEvent).Name;

    public void Send(TEvent evt)
    {
        List<Subscriber<TEvent>> activeHandlers;

        lock (_locker)
        {
            TryRemove();
            activeHandlers = [.. _handlers];
        }

        foreach (var sub in activeHandlers)
            TryInvoke(sub, evt);
    }
    
    public IDisposable Subscribe(Action<TEvent> handler, SubscriptionReference reference = SubscriptionReference.Strong)
    {
        var subscription = new Subscriber<TEvent>(handler, reference);

        lock (_locker)
        {
            _handlers.Add(subscription);
            _logger.LogSubscriberAdded(BrokerType, EventType, Id);
        }

        return new Unsubscriber(() =>
        {
            lock (_locker)
            {
                _handlers.Remove(subscription);
                _logger.LogSubscriberRemoved(BrokerType, EventType, Id);
            }
        });
    }

    private void TryRemove()
    {
        var removedCount = _handlers.RemoveAll(s => !s.IsAlive);

        if (removedCount <= 0)
            return;

        _logger.LogSubscribersRemoved(BrokerType, EventType, Id, removedCount);
    }

    private void TryInvoke(Subscriber<TEvent> sub, TEvent evt)
    {
        try
        {
            sub.Invoke(evt);
        }
        catch (Exception ex)
        {
            _logger.LogErrorInvokingHandler(ex, BrokerType, EventType, Id);
        }
    }
}
