using System.Globalization;
using Messager.NET.Interfaces.Requests;

namespace Messager.NET.AsyncRequests.Services;

public class AsyncRequestWithInputService
{
	public AsyncRequestWithInputService(IAsyncRequest<string, string> request)
	{
		var cancellationToken = CancellationToken.None;
		
		Task.Run(async () =>
		{
			while (!cancellationToken.IsCancellationRequested)
			{
				var result = await request.InvokeAsync(DateTime.Now.ToString(CultureInfo.CurrentCulture));
				
				Console.WriteLine(result);
				Thread.Sleep(1000);
			}
		}, cancellationToken);
	}
}