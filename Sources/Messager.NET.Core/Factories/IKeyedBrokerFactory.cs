using Messager.NET.Receivers;
using Messager.NET.Senders;

namespace Messager.NET.Factories;

/// <summary>
/// Creates or returns brokers for keyed event channels.
/// </summary>
public interface IKeyedBrokerFactory
{
	/// <summary>
	/// Gets a sender for the specified key and event types.
	/// </summary>
	/// <typeparam name="TKey">The routing key type.</typeparam>
	/// <typeparam name="TEvent">The event payload type.</typeparam>
	/// <returns>A sender bound to the specified key and event types.</returns>
	ISender<TKey, TEvent> GetKeyedSender<TKey, TEvent>() where TKey : notnull;

	/// <summary>
	/// Gets a receiver for the specified key and event types.
	/// </summary>
	/// <typeparam name="TKey">The routing key type.</typeparam>
	/// <typeparam name="TEvent">The event payload type.</typeparam>
	/// <returns>A receiver bound to the specified key and event types.</returns>
	IReceiver<TKey, TEvent> GetKeyedReceiver<TKey, TEvent>() where TKey : notnull;

	/// <summary>
	/// Gets an asynchronous sender for the specified key and event types.
	/// </summary>
	/// <typeparam name="TKey">The routing key type.</typeparam>
	/// <typeparam name="TEvent">The event payload type.</typeparam>
	/// <returns>An asynchronous sender bound to the specified key and event types.</returns>
	IAsyncSender<TKey, TEvent> GetAsyncKeyedSender<TKey, TEvent>() where TKey : notnull;

	/// <summary>
	/// Gets an asynchronous receiver for the specified key and event types.
	/// </summary>
	/// <typeparam name="TKey">The routing key type.</typeparam>
	/// <typeparam name="TEvent">The event payload type.</typeparam>
	/// <returns>An asynchronous receiver bound to the specified key and event types.</returns>
	IAsyncReceiver<TKey, TEvent> GetAsyncKeyedReceiver<TKey, TEvent>() where TKey : notnull;
}
