namespace Messager.NET.Senders;

/// <summary>
/// Sends events to subscribers registered for a specific event type.
/// </summary>
/// <typeparam name="TEvent">The event payload type.</typeparam>
public interface ISender<in TEvent>
{
	/// <summary>
	/// Publishes an event to all subscribers of <typeparamref name="TEvent"/>.
	/// </summary>
	/// <param name="evt">The event instance to publish.</param>
	public void Send(TEvent evt);
}

/// <summary>
/// Sends keyed events to subscribers registered for both a key and an event type.
/// </summary>
/// <typeparam name="TKey">The routing key type.</typeparam>
/// <typeparam name="TEvent">The event payload type.</typeparam>
public interface ISender<in TKey, in TEvent> where TKey : notnull
{
	/// <summary>
	/// Publishes an event to subscribers bound to the specified <paramref name="key"/>.
	/// </summary>
	/// <param name="key">The routing key used to select subscribers.</param>
	/// <param name="evt">The event instance to publish.</param>
	public void Send(TKey key, TEvent evt);
}
