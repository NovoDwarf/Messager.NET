using Messager.NET.Brokers;
using Messager.NET.Entities;
using Messager.NET.Enums;
using Messager.NET.Extensions;
using Messager.NET.Receivers;
using Messager.NET.Senders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Messager.NET.Models.Brokers;

/// <summary>Dispatches events to registered subscribers.</summary>
public class SimpleBroker<TEvent> : ISimpleBroker, ISender<TEvent>, IReceiver<TEvent>
{
	private readonly Lock _locker = new();

	private Subscriber<TEvent>[] _handlers = [];

	private readonly ILogger<SimpleBroker<TEvent>> _logger;

	/// <summary>Creates a broker with an optional logger.</summary>
	public SimpleBroker(ILogger<SimpleBroker<TEvent>>? logger = null)
	{
		_logger = logger ?? NullLogger<SimpleBroker<TEvent>>.Instance;
	}

	/// <summary>Gets the unique identifier of this broker.</summary>
	public Guid Id { get; } = Guid.NewGuid();

	/// <summary>Gets the broker type name used in diagnostics.</summary>
	public string BrokerType => typeof(SimpleBroker<>).Name;
	/// <summary>Gets the event type name used in diagnostics.</summary>
	public string EventType => typeof(TEvent).Name;

	/// <inheritdoc />
	public void Send(TEvent evt)
	{
		var handlers = Volatile.Read(ref _handlers);

		var hasDeadSubscriptions = false;

		foreach (var handler in handlers)
			hasDeadSubscriptions |= !TryInvoke(handler, evt);

		if (!hasDeadSubscriptions)
			return;

		lock (_locker)
		{
			var current = _handlers;
			var live = Array.FindAll(current, static handler => handler.IsAlive);

			if (live.Length != current.Length)
				Volatile.Write(ref _handlers, live);
		}
	}

	/// <inheritdoc />
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

			if (removed) _logger.LogSubscriberRemoved(BrokerType, EventType, Id);
		});
	}

	private bool TryInvoke(Subscriber<TEvent> subscription, TEvent evt)
	{
		try
		{
			return subscription.Invoke(evt);
		}
		catch (Exception exception)
		{
			_logger.LogHandlerFailed(exception, BrokerType, EventType, Id);
			return true;
		}
	}
}