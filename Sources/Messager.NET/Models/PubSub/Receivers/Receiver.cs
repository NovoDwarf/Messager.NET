using Messager.NET.Factories;
using Messager.NET.Receivers;

namespace Messager.NET.Models.PubSub.Receivers;

public class Receiver<TEvent> : IReceiver<TEvent>
{
	private readonly IReceiver<TEvent> _impl;

	public Receiver(IBrokerFactory factory)
	{
		_impl = factory.GetReceiver<TEvent>();
	}

	public IDisposable Subscribe(Action<TEvent> handler) => _impl.Subscribe(handler);
}
