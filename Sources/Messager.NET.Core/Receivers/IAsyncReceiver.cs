using Messager.NET.Enums;

namespace Messager.NET.Receivers;

/// <summary>
///     Receives events asynchronously by registering callbacks.
/// </summary>
/// <typeparam name="TEvent">The event payload type.</typeparam>
public interface IAsyncReceiver<out TEvent>
{
	/// <summary>
	///     Subscribes an asynchronous handler with explicit callback-target ownership.
	/// </summary>
	/// <param name="handler">The callback invoked for each event.</param>
	/// <param name="reference">Whether the broker retains the callback target.</param>
	/// <returns>A handle that removes this subscription when disposed asynchronously.</returns>
	public IAsyncDisposable Subscribe(Func<TEvent, ValueTask> handler,
		SubscriptionReference reference = SubscriptionReference.Strong);

	/// <summary>
	///     Removes a previously registered asynchronous handler.
	/// </summary>
	/// <param name="handler">The handler instance to remove.</param>
	public void Unsubscribe(Func<TEvent, ValueTask> handler);
}

/// <summary>
///     Receives keyed events asynchronously by registering callbacks for a specific key and event type.
/// </summary>
/// <typeparam name="TKey">The routing key type.</typeparam>
/// <typeparam name="TEvent">The event payload type.</typeparam>
public interface IAsyncReceiver<in TKey, out TEvent> where TKey : notnull
{
	/// <summary>
	///     Subscribes a keyed asynchronous handler with explicit callback-target ownership.
	/// </summary>
	/// <param name="key">The routing key for the subscription.</param>
	/// <param name="handler">The callback invoked for matching events.</param>
	/// <param name="reference">Whether the broker retains the callback target.</param>
	/// <returns>A handle that removes this subscription when disposed asynchronously.</returns>
	public IAsyncDisposable Subscribe(TKey key, Func<TEvent, ValueTask> handler,
		SubscriptionReference reference = SubscriptionReference.Strong);

	/// <summary>
	///     Removes a previously registered asynchronous handler for the specified key.
	/// </summary>
	/// <param name="key">The routing key used when the handler was registered.</param>
	/// <param name="handler">The handler instance to remove.</param>
	public void Unsubscribe(TKey key, Func<TEvent, ValueTask> handler);
}