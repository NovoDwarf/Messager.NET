using Messager.NET.Factories;
using Messager.NET.Senders;

namespace Messager.NET.Models.PubSub.Senders;

public class AsyncKeyedSender<TKey, TEvent> : IAsyncSender<TKey, TEvent>
	where TKey : notnull
{
	private readonly IAsyncSender<TKey, TEvent> _impl;

	public AsyncKeyedSender(IKeyedBrokerFactory factory)
	{
		_impl = factory.GetAsyncKeyedSender<TKey, TEvent>();
	}

	public ValueTask SendAsync(TKey key, TEvent evt) => _impl.SendAsync(key, evt);
}
