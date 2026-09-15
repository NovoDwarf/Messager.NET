using Messager.NET.Requests;

namespace Messager.NET.AsyncRequests.Services;

public class AsyncRequestService
{
	public AsyncRequestService(IAsyncRequest<string> request)
	{
		var cancellationToken = CancellationToken.None;
		
		Task.Run(async () =>
		{
			while (!cancellationToken.IsCancellationRequested)
			{
				var result = await request.InvokeAsync();
				
				Console.WriteLine(result);
				Thread.Sleep(1000);
			}
		}, cancellationToken);
	}
}