namespace Messager.NET.Requests.Models;

public class SimpleRequest : IRequest<string>
{
	public string Invoke()
	{
		return $"IRequest<string> return {Guid.NewGuid()}";
	}
}