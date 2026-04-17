using Messager.NET.Factories;
using Messager.NET.Senders;

namespace Messager.NET.Models.PubSub.Senders;

public class AsyncSender<TEvent> : IAsyncSender<TEvent>
{
	private readonly IAsyncSender<TEvent> _impl;

	public AsyncSender(IBrokerFactory factory)
	{
		_impl = factory.GetAsyncSender<TEvent>();
	}

	public ValueTask SendAsync(TEvent evt) => _impl.SendAsync(evt);
}
