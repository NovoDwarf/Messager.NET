using Messager.NET.AsyncKeyed.Models.Events;
using Messager.NET.Interfaces.Senders;

namespace Messager.NET.AsyncKeyed.Models.Services;

public class AsyncKeyedSenderService
{
	public AsyncKeyedSenderService(ISender<string, SimpleEvent> sender)
	{
		var cancellationToken = CancellationToken.None;
		
		Task.Run(() =>
		{
			while (!cancellationToken.IsCancellationRequested)
			{
				Thread.Sleep(1000);
				
				sender.Send("key", new SimpleEvent(Guid.NewGuid().ToString(), new ValueTask(Task.CompletedTask)));
				sender.Send("another-key", new SimpleEvent(Guid.NewGuid().ToString(), new ValueTask(Task.CompletedTask)));
			}
		}, cancellationToken);
	}
}