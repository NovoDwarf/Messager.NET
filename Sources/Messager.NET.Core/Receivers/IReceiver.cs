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
    /// <exception cref="NotSupportedException">The receiver does not support explicit reference modes.</exception>
    public IDisposable Subscribe(Action<TEvent> handler, SubscriptionReference reference = SubscriptionReference.Strong)
	{
		throw new NotSupportedException("This receiver does not support explicit subscription references.");
	}
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
    /// <exception cref="NotSupportedException">The receiver does not support explicit reference modes.</exception>
    public IDisposable Subscribe(TKey key, Action<TEvent> handler,
		SubscriptionReference reference = SubscriptionReference.Strong)
	{
		throw new NotSupportedException("This receiver does not support explicit subscription references.");
	}
}