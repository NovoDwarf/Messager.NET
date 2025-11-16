using Messager.NET.Interfaces.Requests;

namespace Messager.NET.Requests.Services;

public class SimpleRequestService
{
	public SimpleRequestService(IRequest<string> request)
	{
		var cancellationToken = CancellationToken.None;
		
		Task.Run(() =>
		{
			while (!cancellationToken.IsCancellationRequested)
			{
				var result = request.Invoke();
				
				Console.WriteLine(result);
				Thread.Sleep(1000);
			}
		}, cancellationToken);
	}
}