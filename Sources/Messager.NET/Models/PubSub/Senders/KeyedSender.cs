using Messager.NET.Factories;
using Messager.NET.Senders;

namespace Messager.NET.Models.PubSub.Senders;

public class KeyedSender<TKey, TEvent> : ISender<TKey, TEvent>
	where TKey : notnull
{
	private readonly ISender<TKey, TEvent> _impl;

	public KeyedSender(IKeyedBrokerFactory factory)
	{
		_impl = factory.GetKeyedSender<TKey, TEvent>();
	}

	public void Send(TKey key, TEvent evt) => _impl.Send(key, evt);
}
