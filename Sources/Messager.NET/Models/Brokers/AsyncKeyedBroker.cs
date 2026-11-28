using Messager.NET.Brokers;
using Messager.NET.Entities;
using Messager.NET.Enums;
using Messager.NET.Extensions;
using Messager.NET.Receivers;
using Messager.NET.Senders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Messager.NET.Models.Brokers;

/// <summary>Dispatches asynchronous keyed events to registered subscribers.</summary>
public class AsyncKeyedBroker<TKey, TEvent> : IKeyedBroker, IAsyncSender<TKey, TEvent>, IAsyncReceiver<TKey, TEvent>
	where TKey : notnull
{
	private readonly Dictionary<TKey, SubscriberAsync<TEvent>[]> _handlers = new();
	private readonly Lock _locker = new();

	private readonly ILogger<AsyncKeyedBroker<TKey, TEvent>> _logger;

	/// <summary>Creates a broker with an optional logger.</summary>
	public AsyncKeyedBroker(ILogger<AsyncKeyedBroker<TKey, TEvent>>? logger = null)
	{
		_logger = logger ?? NullLogger<AsyncKeyedBroker<TKey, TEvent>>.Instance;
	}

	/// <summary>Gets the unique identifier of this broker.</summary>
	public Guid Id { get; } = Guid.NewGuid();

	/// <summary>Gets the broker type name used in diagnostics.</summary>
	public string BrokerType => typeof(AsyncKeyedBroker<,>).Name;
	/// <summary>Gets the routing key type name used in diagnostics.</summary>
	public string KeyType => typeof(TKey).Name;
	/// <summary>Gets the event type name used in diagnostics.</summary>
	public string EventType => typeof(TEvent).Name;

	/// <inheritdoc />
	public IAsyncDisposable Subscribe(TKey key, Func<TEvent, ValueTask> handler, SubscriptionReference reference = SubscriptionReference.Strong)
	{
		var subscription = new SubscriberAsync<TEvent>(handler, reference);
		var created = false;
		var count = 0;

		lock (_locker)
		{
			created = !_handlers.TryGetValue(key, out var current);
			current ??= [];

			var updated = new SubscriberAsync<TEvent>[
				current.Length + 1];

			Array.Copy(current, updated, current.Length);
			updated[^1] = subscription;

			_handlers[key] = updated;
			count = updated.Length;
		}

		if (created)
			_logger.LogKeyedSubscriptionListCreated(BrokerType, KeyType, EventType, Id, key);

		_logger.LogKeyedSubscriberAdded(BrokerType, KeyType, EventType, Id, key, count);

		var weakSubscription = new WeakReference<SubscriberAsync<TEvent>>(subscription);
		return new UnsubscriberAsync(() =>
		{
			if (weakSubscription.TryGetTarget(out var target))
				RemoveSubscription(key, target);
			return ValueTask.CompletedTask;
		});
	}

	/// <inheritdoc />
	public void Unsubscribe(TKey key, Func<TEvent, ValueTask> handler)
	{
		ArgumentNullException.ThrowIfNull(handler);

		var removed = false;
		var removedKey = false;
		var remainingCount = 0;

		lock (_locker)
		{
			if (_handlers.TryGetValue(key, out var current))
			{
				var index = Array.FindIndex(current, subscription => subscription.Matches(handler));

				if (index >= 0)
				{
					RemoveAt(key, current, index, out removedKey, out remainingCount);
					removed = true;
				}
			}
		}

		LogRemoval(key, removed, removedKey, remainingCount);
	}

	/// <inheritdoc />
	public async ValueTask SendAsync(TKey key, TEvent evt)
	{
		SubscriberAsync<TEvent>[]? handlers;

		lock (_locker)
		{
			if (!_handlers.TryGetValue(key, out handlers))
				return;
		}

		var hasDeadSubscriptions = false;
		foreach (var handler in handlers)
		{
			hasDeadSubscriptions |= !handler.IsAlive;
			await TryInvokeAsync(handler, key, evt).ConfigureAwait(false);
		}

		if (hasDeadSubscriptions)
        {
            lock (_locker)
			{
				if (_handlers.TryGetValue(key, out var current))
				{
					var live = Array.FindAll(current, static handler => handler.IsAlive);
					
					if (live.Length == 0)
						_handlers.Remove(key);
					else if (live.Length != current.Length)
						_handlers[key] = live;
				}
			}
        }
    }

	private void RemoveSubscription(TKey key, SubscriberAsync<TEvent> subscription)
	{
		var removed = false;
		var removedKey = false;
		var remainingCount = 0;

		lock (_locker)
		{
			if (_handlers.TryGetValue(key, out var current))
			{
				var index = Array.IndexOf(current, subscription);

				if (index >= 0)
				{
					RemoveAt(key, current, index, out removedKey, out remainingCount);
					removed = true;
				}
			}
		}

		LogRemoval(key, removed, removedKey, remainingCount);
	}

	private void RemoveAt(TKey key, SubscriberAsync<TEvent>[] current, int index, out bool removedKey,
		out int remainingCount)
	{
		remainingCount = current.Length - 1;
		removedKey = remainingCount == 0;

		if (removedKey)
		{
			_handlers.Remove(key);
			return;
		}

		var updated = new SubscriberAsync<TEvent>[remainingCount];

		if (index > 0)
			Array.Copy(current, 0, updated, 0, index);

		if (index < updated.Length)
			Array.Copy(current, index + 1, updated, index, updated.Length - index);

		_handlers[key] = updated;
	}

	private void LogRemoval(TKey key, bool removed, bool removedKey, int remainingCount)
	{
		if (removed)
			_logger.LogKeyedSubscriberRemoved(BrokerType, KeyType, EventType, Id, key, remainingCount);

		if (removedKey)
			_logger.LogKeyedSubscriptionListRemoved(BrokerType, KeyType, EventType, Id, key);
	}

	private async ValueTask TryInvokeAsync(SubscriberAsync<TEvent> handler, TKey key, TEvent evt)
	{
		try
		{
			await handler.InvokeAsync(evt).ConfigureAwait(false);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error invoking async handler for event {EventType} with key {Key}", EventType, key);
		}
	}
}