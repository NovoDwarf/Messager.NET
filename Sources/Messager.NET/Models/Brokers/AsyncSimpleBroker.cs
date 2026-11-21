using Messager.NET.Brokers;
using Messager.NET.Entities;
using Messager.NET.Extensions;
using Messager.NET.Receivers;
using Messager.NET.Senders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Messager.NET.Models.Brokers;


public class AsyncSimpleBroker<TEvent> : ISimpleBroker, IAsyncSender<TEvent>, IAsyncReceiver<TEvent>
{
    private readonly List<SubscriberAsync<TEvent>> _handlers = [];
    private readonly Lock _locker = new();

    private readonly ILogger<AsyncSimpleBroker<TEvent>> _logger;

    public AsyncSimpleBroker(ILogger<AsyncSimpleBroker<TEvent>>? logger = null)
    {
        _logger = logger ??
            NullLogger<AsyncSimpleBroker<TEvent>>.Instance;
    }

    public Guid Id { get; } = Guid.NewGuid();

    public string BrokerType => typeof(AsyncSimpleBroker<>).Name;
    public string EventType => typeof(TEvent).Name;

    public async ValueTask SendAsync(TEvent evt)
    {
        SubscriberAsync<TEvent>[] handlers;
        var removedCount = 0;

        lock (_locker)
        {
            removedCount = _handlers.RemoveAll(subscription => !subscription.IsAlive);
            handlers = [.. _handlers];
        }

        if (removedCount > 0) 
            _logger.LogSubscribersRemoved(BrokerType, EventType, Id, removedCount);

        foreach (var handler in handlers) 
            await TryInvokeAsync(handler, evt).ConfigureAwait(false);
    }

    public IAsyncDisposable Subscribe(Func<TEvent, ValueTask> handler, SubscriptionReference reference)
    {
        var subscription = new SubscriberAsync<TEvent>(handler, reference);

        lock (_locker)
            _handlers.Add(subscription);

        _logger.LogSubscriberAdded(BrokerType, EventType, Id);

        return new UnsubscriberAsync(() =>
        {
            var removed = false;

            lock (_locker)
                removed = _handlers.Remove(subscription);

            if (removed) 
                _logger.LogSubscriberRemoved(BrokerType, EventType, Id);

            return ValueTask.CompletedTask;
        });
    }

    public void Unsubscribe(Func<TEvent, ValueTask> handler)
    {
        var removed = false;

        lock (_locker)
        {
            var index = _handlers.FindIndex(subscription => subscription.Matches(handler));

            if (index >= 0)
            {
                _handlers.RemoveAt(index);
                removed = true;
            }
        }

        if (removed) 
            _logger.LogSubscriberRemoved(BrokerType, EventType, Id);
    }

    private async ValueTask TryInvokeAsync(SubscriberAsync<TEvent> subscription, TEvent evt)
    {
        try
        {
            await subscription.InvokeAsync(evt).ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            _logger.LogHandlerFailed(exception, BrokerType, EventType, Id);
        }
    }
}

