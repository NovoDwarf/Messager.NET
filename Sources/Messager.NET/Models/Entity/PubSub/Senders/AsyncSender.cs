using Messager.NET.Interfaces.Factories;
using Messager.NET.Interfaces.Senders;

namespace Messager.NET.Models.Entity.PubSub.Senders;

public class AsyncSender<TEvent> : IAsyncSender<TEvent>
{
	private readonly IAsyncSender<TEvent> _impl;

	public AsyncSender(IBrokerFactory factory)
	{
		_impl = factory.GetAsyncSender<TEvent>();
	}

	public ValueTask SendAsync(TEvent evt) => _impl.SendAsync(evt);
}
