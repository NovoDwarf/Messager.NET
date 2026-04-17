namespace Messager.NET.Senders;

/// <summary>
/// Sends events asynchronously to subscribers registered for a specific event type.
/// </summary>
/// <typeparam name="TEvent">The event payload type.</typeparam>
public interface IAsyncSender<in TEvent>
{
	/// <summary>
	/// Publishes an event to all asynchronous subscribers of <typeparamref name="TEvent"/>.
	/// </summary>
	/// <param name="evt">The event instance to publish.</param>
	/// <returns>A task that completes when delivery finishes.</returns>
	public ValueTask SendAsync(TEvent evt);
}

/// <summary>
/// Sends keyed events asynchronously to subscribers registered for both a key and an event type.
/// </summary>
/// <typeparam name="TKey">The routing key type.</typeparam>
/// <typeparam name="TEvent">The event payload type.</typeparam>
public interface IAsyncSender<in TKey, in TEvent> where TKey : notnull
{
	/// <summary>
	/// Publishes an event to asynchronous subscribers bound to the specified <paramref name="key"/>.
	/// </summary>
	/// <param name="key">The routing key used to select subscribers.</param>
	/// <param name="evt">The event instance to publish.</param>
	/// <returns>A task that completes when delivery finishes.</returns>
	public ValueTask SendAsync(TKey key, TEvent evt);
}
