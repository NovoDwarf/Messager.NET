using Messager.NET.Interfaces.Requests;

namespace Messager.NET.Requests.Services;

public class SimpleRequestService
{
	public SimpleRequestService(IRequest<string, int> request)
	{
		var cancellationToken = CancellationToken.None;
		
		Task.Run(() =>
		{
			while (!cancellationToken.IsCancellationRequested)
			{
				var result = request.Invoke(1);
				
				Console.WriteLine(result);
				Thread.Sleep(1000);
			}
		}, cancellationToken);
	}
}