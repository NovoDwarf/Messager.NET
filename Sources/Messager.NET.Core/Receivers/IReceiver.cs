using Messager.NET.Enums;

namespace Messager.NET.Receivers;

/// <summary>
///     Receives events of a specific type by registering callbacks.
/// </summary>
/// <typeparam name="TEvent">The event payload type.</typeparam>
public interface IReceiver<out TEvent>
{
	/// <summary>
	///     Subscribes a handler with explicit callback-target ownership.
	/// </summary>
	/// <param name="handler">The callback invoked for each event.</param>
	/// <param name="reference">Whether the broker retains the callback target.</param>
	/// <returns>A handle that removes this subscription when disposed.</returns>
	public IDisposable Subscribe(Action<TEvent> handler,
		SubscriptionReference reference = SubscriptionReference.Strong);
}

/// <summary>
///     Receives keyed events by registering callbacks for a specific key and event type.
/// </summary>
/// <typeparam name="TKey">The routing key type.</typeparam>
/// <typeparam name="TEvent">The event payload type.</typeparam>
public interface IReceiver<in TKey, out TEvent>
{
	/// <summary>
	///     Subscribes a keyed handler with explicit callback-target ownership.
	/// </summary>
	/// <param name="key">The routing key for the subscription.</param>
	/// <param name="handler">The callback invoked for matching events.</param>
	/// <param name="reference">Whether the broker retains the callback target.</param>
	/// <returns>A handle that removes this subscription when disposed.</returns>
	public IDisposable Subscribe(TKey key, Action<TEvent> handler,
		SubscriptionReference reference = SubscriptionReference.Strong);
}