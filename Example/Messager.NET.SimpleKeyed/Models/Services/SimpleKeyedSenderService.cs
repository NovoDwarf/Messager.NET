using Messager.NET.Interfaces.Senders;
using Messager.NET.SimpleKeyed.Models.Events;

namespace Messager.NET.SimpleKeyed.Models.Services;

public class SimpleKeyedSenderService
{
	public SimpleKeyedSenderService(ISender<string, SimpleEvent> sender)
	{
		var cancellationToken = CancellationToken.None;
		
		Task.Run(() =>
		{
			while (!cancellationToken.IsCancellationRequested)
			{
				Thread.Sleep(1000);
				
				sender.Send("key", new SimpleEvent(Guid.NewGuid().ToString()));
				sender.Send("another-key", new SimpleEvent(Guid.NewGuid().ToString()));
			}
		}, cancellationToken);
	}
}