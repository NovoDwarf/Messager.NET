using Messager.NET.Enums;
using Messager.NET.Factories;
using Messager.NET.Receivers;

namespace Messager.NET.Models.PubSub.Receivers;

/// <summary>Delegates subscriptions to a broker obtained from the supplied factory.</summary>
public class AsyncKeyedReceiver<TKey, TEvent> : IAsyncReceiver<TKey, TEvent>
	where TKey : notnull
{
	private readonly IAsyncReceiver<TKey, TEvent> _impl;

	/// <summary>Creates an instance using the specified broker factory.</summary>
	public AsyncKeyedReceiver(IKeyedBrokerFactory factory)
	{
		_impl = factory.GetAsyncKeyedReceiver<TKey, TEvent>();
	}

	/// <inheritdoc />
	public IAsyncDisposable Subscribe(TKey key, Func<TEvent, ValueTask> handler, SubscriptionReference reference = SubscriptionReference.Strong)
	{
		return _impl.Subscribe(key, handler, reference);
	}

	/// <inheritdoc />
	public void Unsubscribe(TKey key, Func<TEvent, ValueTask> handler)
	{
		_impl.Unsubscribe(key, handler);
	}
}