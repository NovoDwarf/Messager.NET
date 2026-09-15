using Messager.NET.Receivers;
using Messager.NET.SimpleKeyed.Models.Events;

namespace Messager.NET.SimpleKeyed.Models.Services;

public sealed class KeyedReceiverService : IDisposable
{
	private readonly IDisposable _subscription;
	
	public KeyedReceiverService(IReceiver<string, SimpleEvent> receiver)
	{
		_subscription = receiver.Subscribe("key", OnMessageReceived);
	}
	
	public void Dispose() => _subscription.Dispose();

	private static void OnMessageReceived(SimpleEvent evt) => Console.WriteLine($"Message received: {evt.Message}");
}