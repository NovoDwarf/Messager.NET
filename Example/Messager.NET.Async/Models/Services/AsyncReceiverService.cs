using Messager.NET.Async.Models.Events;
using Messager.NET.Interfaces.Receivers;

namespace Messager.NET.Async.Models.Services;

public sealed class AsyncReceiverService : IAsyncDisposable
{
	private readonly IAsyncDisposable _subscription;
	
	public AsyncReceiverService(IAsyncReceiver<SimpleEvent> receiver)
	{
		_subscription = receiver.Subscribe(OnMessageReceived);
	}
	
	public ValueTask DisposeAsync()
	{
		return _subscription.DisposeAsync();
	}

	private static ValueTask OnMessageReceived(SimpleEvent evt)
	{
		Console.WriteLine($"Message received: {evt.Message}");
		
		return evt.Task;
	}
}