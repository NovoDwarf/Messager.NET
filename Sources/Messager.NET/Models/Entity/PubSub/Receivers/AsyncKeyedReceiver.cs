using Messager.NET.Interfaces.Factories;
using Messager.NET.Interfaces.Receivers;

namespace Messager.NET.Models.Entity.PubSub.Receivers;

public class AsyncKeyedReceiver<TKey, TEvent> : IAsyncReceiver<TKey, TEvent>
	where TKey : notnull
{
	private readonly IAsyncReceiver<TKey, TEvent> _impl;

	public AsyncKeyedReceiver(IKeyedBrokerFactory factory)
	{
		_impl = factory.GetAsyncKeyedReceiver<TKey, TEvent>();
	}

	public IAsyncDisposable Subscribe(TKey key, Func<TEvent, ValueTask> handler) => _impl.Subscribe(key, handler);

	public void Unsubscribe(TKey key, Func<TEvent, ValueTask> handler) => _impl.Unsubscribe(key, handler);
}
