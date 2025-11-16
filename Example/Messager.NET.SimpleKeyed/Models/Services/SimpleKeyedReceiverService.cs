using Messager.NET.Interfaces.Receivers;
using Messager.NET.SimpleKeyed.Models.Events;

namespace Messager.NET.SimpleKeyed.Models.Services;

public sealed class SimpleKeyedReceiverService : IDisposable
{
	private readonly IDisposable _subscription;
	
	public SimpleKeyedReceiverService(IReceiver<string, SimpleEvent> receiver)
	{
		_subscription = receiver.Subscribe("key", OnMessageReceived);
	}
	
	public void Dispose()
	{
		_subscription.Dispose();
	}

	private static void OnMessageReceived(SimpleEvent evt)
	{
		Console.WriteLine($"Message received: {evt.Message}");
	}
}