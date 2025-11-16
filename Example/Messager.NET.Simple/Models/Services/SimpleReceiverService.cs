using Messager.NET.Example.Models.Events;
using Messager.NET.Interfaces.Receivers;

namespace Messager.NET.Example.Models.Services;

public sealed class SimpleReceiverService : IDisposable
{
	private readonly IDisposable _subscription;
	
	public SimpleReceiverService(IReceiver<SimpleEvent> receiver)
	{
		_subscription = receiver.Subscribe(OnMessageReceived);
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