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
    private readonly Lock _locker = new();

    private SubscriberAsync<TEvent>[] _handlers = [];

    private readonly ILogger<AsyncSimpleBroker<TEvent>> _logger;

    public AsyncSimpleBroker(ILogger<AsyncSimpleBroker<TEvent>>? logger = null)
    {
        _logger = logger ?? NullLogger<AsyncSimpleBroker<TEvent>>.Instance;
    }

    public Guid Id { get; } = Guid.NewGuid();

    public string BrokerType => typeof(AsyncSimpleBroker<>).Name;
    public string EventType => typeof(TEvent).Name;

    public async ValueTask SendAsync(TEvent evt)
    {
        var handlers = Volatile.Read(ref _handlers);

        foreach (var handler in handlers)
            await TryInvokeAsync(handler, evt).ConfigureAwait(false);
    }

    public IAsyncDisposable Subscribe(Func<TEvent, ValueTask> handler, SubscriptionReference reference)
    {
        var subscription = new SubscriberAsync<TEvent>(handler, reference);

        lock (_locker)
        {
            var current = _handlers;
            var updated = new SubscriberAsync<TEvent>[
                current.Length + 1];

            Array.Copy(current, updated, current.Length);
            updated[^1] = subscription;

            Volatile.Write(ref _handlers, updated);
        }

        _logger.LogSubscriberAdded(BrokerType, EventType, Id);

        var weakSubscription = new WeakReference<SubscriberAsync<TEvent>>(subscription);
        return new UnsubscriberAsync(() =>
        {
            var removed = false;

            if (weakSubscription.TryGetTarget(out var target))
            {
                lock (_locker)
                    removed = RemoveSubscription(target);
            }

            if (removed) 
                _logger.LogSubscriberRemoved(BrokerType, EventType, Id);

            return ValueTask.CompletedTask;
        });
    }

    public void Unsubscribe(Func<TEvent, ValueTask> handler)
    {
        ArgumentNullException.ThrowIfNull(handler);

        var removed = false;

        lock (_locker)
        {
            var current = _handlers;

            for (var i = 0; i < current.Length; i++)
            {
                if (!current[i].Matches(handler))
                    continue;

                PublishWithout(current, i);
                removed = true;
                break;
            }
        }

        if (removed) 
            _logger.LogSubscriberRemoved(BrokerType, EventType, Id);
    }

    private bool RemoveSubscription(SubscriberAsync<TEvent> subscription)
    {
        var current = _handlers;
        var index = Array.IndexOf(current, subscription);

        if (index < 0)
            return false;

        PublishWithout(current, index);
        return true;
    }

    private void PublishWithout(SubscriberAsync<TEvent>[] current, int index)
    {
        var updated = new SubscriberAsync<TEvent>[current.Length - 1];

        if (index > 0) 
            Array.Copy(current, 0, updated, 0, index);

        if (index < updated.Length) 
            Array.Copy(current, index + 1, updated, index, updated.Length - index);

        Volatile.Write(ref _handlers, updated);
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

