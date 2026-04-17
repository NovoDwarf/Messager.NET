using Messager.NET.Receivers;
using Messager.NET.Senders;

namespace Messager.NET.Factories;

/// <summary>
/// Creates or returns brokers for non-keyed event channels.
/// </summary>
public interface IBrokerFactory
{
	/// <summary>
	/// Gets a sender for the specified event type.
	/// </summary>
	/// <typeparam name="TEvent">The event payload type.</typeparam>
	/// <returns>A sender bound to <typeparamref name="TEvent"/>.</returns>
	ISender<TEvent> GetSender<TEvent>();

	/// <summary>
	/// Gets a receiver for the specified event type.
	/// </summary>
	/// <typeparam name="TEvent">The event payload type.</typeparam>
	/// <returns>A receiver bound to <typeparamref name="TEvent"/>.</returns>
	IReceiver<TEvent> GetReceiver<TEvent>();

	/// <summary>
	/// Gets an asynchronous sender for the specified event type.
	/// </summary>
	/// <typeparam name="TEvent">The event payload type.</typeparam>
	/// <returns>An asynchronous sender bound to <typeparamref name="TEvent"/>.</returns>
	IAsyncSender<TEvent> GetAsyncSender<TEvent>();

	/// <summary>
	/// Gets an asynchronous receiver for the specified event type.
	/// </summary>
	/// <typeparam name="TEvent">The event payload type.</typeparam>
	/// <returns>An asynchronous receiver bound to <typeparamref name="TEvent"/>.</returns>
	IAsyncReceiver<TEvent> GetAsyncReceiver<TEvent>();
}
