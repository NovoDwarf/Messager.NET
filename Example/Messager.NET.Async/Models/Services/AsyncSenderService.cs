using Messager.NET.Async.Models.Events;
using Messager.NET.Interfaces.Senders;

namespace Messager.NET.Async.Models.Services;

public class AsyncSenderService
{
	public AsyncSenderService(IAsyncSender<SimpleEvent> sender)
	{
		var cancellationToken = CancellationToken.None;
		
		Task.Run(async () =>
		{
			while (!cancellationToken.IsCancellationRequested)
			{
				Thread.Sleep(1000);
				
				await sender.SendAsync(new SimpleEvent(Guid.NewGuid().ToString(), new ValueTask(Task.CompletedTask)));
			}
		}, cancellationToken);
	}
}