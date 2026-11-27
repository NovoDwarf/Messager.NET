using Messager.NET.Enums;
using Messager.NET.Factories;
using Messager.NET.Receivers;

namespace Messager.NET.Models.PubSub.Receivers;

/// <summary>Delegates subscriptions to a broker obtained from the supplied factory.</summary>
public class Receiver<TEvent> : IReceiver<TEvent>
{
	private readonly IReceiver<TEvent> _impl;

	/// <summary>Creates an instance using the specified broker factory.</summary>
	public Receiver(IBrokerFactory factory)
	{
		_impl = factory.GetReceiver<TEvent>();
	}

	/// <inheritdoc />
	public IDisposable Subscribe(Action<TEvent> handler, SubscriptionReference reference = SubscriptionReference.Strong)
	{
		return _impl.Subscribe(handler, reference);
	}
}