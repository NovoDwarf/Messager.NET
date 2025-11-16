using Messager.NET.Interfaces.Factories;
using Messager.NET.Interfaces.Receivers;

namespace Messager.NET.Models.Entity.PubSub.Receivers;

public class KeyedReceiver<TKey, TEvent> : IReceiver<TKey, TEvent> 
	where TKey : notnull
{
	private readonly IReceiver<TKey, TEvent> _impl;
	
	public KeyedReceiver(IKeyedBrokerFactory factory)
	{
		_impl = factory.GetKeyedReceiver<TKey, TEvent>();
	}

	public IDisposable Subscribe(TKey key, Action<TEvent> handler) => _impl.Subscribe(key, handler);
}