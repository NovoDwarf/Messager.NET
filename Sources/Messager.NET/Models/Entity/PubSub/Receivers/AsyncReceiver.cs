using Messager.NET.Interfaces.Factories;
using Messager.NET.Interfaces.Receivers;

namespace Messager.NET.Models.Entity.PubSub.Receivers;

public class AsyncReceiver<TEvent> : IAsyncReceiver<TEvent>
{
	private readonly IAsyncReceiver<TEvent> _impl;

	public AsyncReceiver(IBrokerFactory factory)
	{
		_impl = factory.GetAsyncReceiver<TEvent>();
	}

	public IAsyncDisposable Subscribe(Func<TEvent, ValueTask> handler) => _impl.Subscribe(handler);

	public void Unsubscribe(Func<TEvent, ValueTask> handler) => _impl.Unsubscribe(handler);
}
