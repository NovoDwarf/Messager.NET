using Messager.NET.Factories;
using Messager.NET.Senders;

namespace Messager.NET.Models.PubSub.Senders;

/// <summary>Delegates event dispatch to a broker obtained from the supplied factory.</summary>
public class KeyedSender<TKey, TEvent> : ISender<TKey, TEvent>
	where TKey : notnull
{
	private readonly ISender<TKey, TEvent> _impl;

	/// <summary>Creates an instance using the specified broker factory.</summary>
	public KeyedSender(IKeyedBrokerFactory factory)
	{
		_impl = factory.GetKeyedSender<TKey, TEvent>();
	}

	/// <inheritdoc />
	public void Send(TKey key, TEvent evt)
	{
		_impl.Send(key, evt);
	}
}