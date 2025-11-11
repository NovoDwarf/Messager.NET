using Messager.NET.Interfaces.Factories;
using Messager.NET.Interfaces.Senders;

namespace Messager.NET.Entity.Senders;

public class KeyedSender<TKey, TEvent> : ISender<TKey, TEvent> where TKey : notnull
{
	private readonly ISender<TKey, TEvent> _impl;
	
	public KeyedSender(IKeyedMessageBrokerFactory factory)
	{
		_impl = factory.GetKeyedSender<TKey, TEvent>();
	}

	public void Send(TKey key, TEvent evt) => _impl.Send(key, evt);
}