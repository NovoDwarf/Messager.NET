namespace Messager.NET.Receivers;

/// <summary>
/// Receives events of a specific type by registering callbacks.
/// </summary>
/// <typeparam name="TEvent">The event payload type.</typeparam>
public interface IReceiver<out TEvent>
{
	/// <summary>
	/// Subscribes a handler to events of type <typeparamref name="TEvent"/>.
	/// </summary>
	/// <param name="handler">The callback invoked for each matching event.</param>
	/// <returns>A disposable handle that removes the subscription.</returns>
	public IDisposable Subscribe(Action<TEvent> handler);
}

/// <summary>
/// Receives keyed events by registering callbacks for a specific key and event type.
/// </summary>
/// <typeparam name="TKey">The routing key type.</typeparam>
/// <typeparam name="TEvent">The event payload type.</typeparam>
public interface IReceiver<in TKey, out TEvent>
{
	/// <summary>
	/// Subscribes a handler to events matching the specified <paramref name="key"/>.
	/// </summary>
	/// <param name="key">The routing key used to bind the subscription.</param>
	/// <param name="handler">The callback invoked for each matching event.</param>
	/// <returns>A disposable handle that removes the subscription.</returns>
	public IDisposable Subscribe(TKey key, Action<TEvent> handler);
}
