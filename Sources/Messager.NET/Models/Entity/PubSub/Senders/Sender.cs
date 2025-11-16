using Messager.NET.Interfaces.Factories;
using Messager.NET.Interfaces.Senders;

namespace Messager.NET.Models.Entity.PubSub.Senders;

public class Sender<TEvent> : ISender<TEvent>
{
	private readonly ISender<TEvent> _impl;
	
	public Sender(IBrokerFactory factory)
	{
		_impl = factory.GetSender<TEvent>();
	}

	public void Send(TEvent evt) => _impl.Send(evt);
}