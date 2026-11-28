using Messager.NET.Factories;
using Messager.NET.Senders;

namespace Messager.NET.Models.PubSub.Senders;

/// <summary>Delegates event dispatch to a broker obtained from the supplied factory.</summary>
public class AsyncSender<TEvent> : IAsyncSender<TEvent>
{
	private readonly IAsyncSender<TEvent> _impl;

	/// <summary>Creates an instance using the specified broker factory.</summary>
	public AsyncSender(IBrokerFactory factory)
	{
		_impl = factory.GetAsyncSender<TEvent>();
	}

	/// <inheritdoc />
	public ValueTask SendAsync(TEvent evt)
	{
		return _impl.SendAsync(evt);
	}
}