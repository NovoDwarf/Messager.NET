using Messager.NET.AsyncKeyed.Models.Events;
using Messager.NET.Interfaces.Receivers;

namespace Messager.NET.AsyncKeyed.Models.Services;

public sealed class AsyncKeyedReceiverService : IAsyncDisposable
{
	private readonly IAsyncDisposable _subscription;
	
	public AsyncKeyedReceiverService(IAsyncReceiver<string, SimpleEvent> receiver)
	{
		_subscription = receiver.Subscribe("key", OnMessageReceived);
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