using Messager.NET.Factories;
using Messager.NET.Senders;

namespace Messager.NET.Models.PubSub.Senders;

/// <summary>Delegates event dispatch to a broker obtained from the supplied factory.</summary>
public class AsyncKeyedSender<TKey, TEvent> : IAsyncSender<TKey, TEvent>
	where TKey : notnull
{
	private readonly IAsyncSender<TKey, TEvent> _impl;

	/// <summary>Creates an instance using the specified broker factory.</summary>
	public AsyncKeyedSender(IKeyedBrokerFactory factory)
	{
		_impl = factory.GetAsyncKeyedSender<TKey, TEvent>();
	}

	/// <inheritdoc />
	public ValueTask SendAsync(TKey key, TEvent evt)
	{
		return _impl.SendAsync(key, evt);
	}
}