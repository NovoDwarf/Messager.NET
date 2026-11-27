using Messager.NET.Factories;
using Messager.NET.Senders;

namespace Messager.NET.Models.PubSub.Senders;

/// <summary>Delegates event dispatch to a broker obtained from the supplied factory.</summary>
public class Sender<TEvent> : ISender<TEvent>
{
	private readonly ISender<TEvent> _impl;

	/// <summary>Creates an instance using the specified broker factory.</summary>
	public Sender(IBrokerFactory factory)
	{
		_impl = factory.GetSender<TEvent>();
	}

	/// <inheritdoc />
	public void Send(TEvent evt)
	{
		_impl.Send(evt);
	}
}