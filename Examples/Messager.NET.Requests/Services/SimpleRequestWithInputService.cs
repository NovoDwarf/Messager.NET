using System.Globalization;

namespace Messager.NET.Requests.Services;

public class SimpleRequestWithInputService
{
	public SimpleRequestWithInputService(IRequest<string, string> request)
	{
		var cancellationToken = CancellationToken.None;
		
		Task.Run(() =>
		{
			while (!cancellationToken.IsCancellationRequested)
			{
				var result = request.Invoke(DateTime.Now.ToString(CultureInfo.CurrentCulture));
				
				Console.WriteLine(result);
				Thread.Sleep(1000);
			}
		}, cancellationToken);
	}
}