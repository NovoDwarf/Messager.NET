using Messager.NET.Enums;
using Messager.NET.Factories;
using Messager.NET.Receivers;

namespace Messager.NET.Models.PubSub.Receivers;

/// <summary>Delegates subscriptions to a broker obtained from the supplied factory.</summary>
public class AsyncReceiver<TEvent> : IAsyncReceiver<TEvent>
{
	private readonly IAsyncReceiver<TEvent> _impl;

	/// <summary>Creates an instance using the specified broker factory.</summary>
	public AsyncReceiver(IBrokerFactory factory)
	{
		_impl = factory.GetAsyncReceiver<TEvent>();
	}

	/// <inheritdoc />
	public IAsyncDisposable Subscribe(Func<TEvent, ValueTask> handler, SubscriptionReference reference = SubscriptionReference.Strong)
	{
		return _impl.Subscribe(handler, reference);
	}

	/// <inheritdoc />
	public void Unsubscribe(Func<TEvent, ValueTask> handler)
	{
		_impl.Unsubscribe(handler);
	}
}