using Messager.NET.Interfaces.Factories;
using Messager.NET.Interfaces.Receivers;

namespace Messager.NET.Entity.Receivers;

public class Receiver<TEvent> : IReceiver<TEvent>
{
	private readonly IReceiver<TEvent> _impl;
	
	public Receiver(IMessageBrokerFactory factory)
	{
		_impl = factory.GetReceiver<TEvent>();
	}

	public IDisposable Subscribe(Action<TEvent> handler) => _impl.Subscribe(handler);
}