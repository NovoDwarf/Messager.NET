using Messager.NET.Enums;
using Messager.NET.Factories;
using Messager.NET.Receivers;

namespace Messager.NET.Models.PubSub.Receivers;

/// <summary>Delegates subscriptions to a broker obtained from the supplied factory.</summary>
public class KeyedReceiver<TKey, TEvent> : IReceiver<TKey, TEvent>
	where TKey : notnull
{
	private readonly IReceiver<TKey, TEvent> _impl;

	/// <summary>Creates an instance using the specified broker factory.</summary>
	public KeyedReceiver(IKeyedBrokerFactory factory)
	{
		_impl = factory.GetKeyedReceiver<TKey, TEvent>();
	}

	/// <inheritdoc />
	public IDisposable Subscribe(TKey key, Action<TEvent> handler, SubscriptionReference reference = SubscriptionReference.Strong)
	{
		return _impl.Subscribe(key, handler, reference);
	}
}