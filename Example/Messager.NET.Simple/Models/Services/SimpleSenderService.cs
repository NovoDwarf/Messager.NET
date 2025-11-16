using Messager.NET.Example.Models.Events;
using Messager.NET.Interfaces.Senders;

namespace Messager.NET.Example.Models.Services;

public class SimpleSenderService
{
	public SimpleSenderService(ISender<SimpleEvent> sender)
	{
		var cancellationToken = CancellationToken.None;
		
		Task.Run(() =>
		{
			while (!cancellationToken.IsCancellationRequested)
			{
				Thread.Sleep(1000);
				
				sender.Send(new SimpleEvent(Guid.NewGuid().ToString()));
			}
		}, cancellationToken);
	}
}