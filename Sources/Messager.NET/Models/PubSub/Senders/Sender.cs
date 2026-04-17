using Messager.NET.Factories;
using Messager.NET.Senders;

namespace Messager.NET.Models.PubSub.Senders;

public class Sender<TEvent> : ISender<TEvent>
{
	private readonly ISender<TEvent> _impl;

	public Sender(IBrokerFactory factory)
	{
		_impl = factory.GetSender<TEvent>();
	}

	public void Send(TEvent evt) => _impl.Send(evt);
}
