using Messager.NET.Interfaces.Factories;
using Messager.NET.Interfaces.Receivers;

namespace Messager.NET.Entity.Receivers;

public class KeyedReceiver<TKey, TEvent> : IReceiver<TKey, TEvent> 
	where TKey : notnull
{
	private readonly IReceiver<TKey, TEvent> _impl;
	
	public KeyedReceiver(IKeyedMessageBrokerFactory factory)
	{
		_impl = factory.GetKeyedReceiver<TKey, TEvent>();
	}

	public IDisposable Subscribe(TKey key, Action<TEvent> handler) => _impl.Subscribe(key, handler);
}