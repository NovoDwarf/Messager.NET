using Messager.NET.Interfaces.Requests;

namespace Messager.NET.Requests.Requests;

public class SimpleRequest : IRequest<string>
{
	public string Invoke()
	{
		return $"return {Guid.NewGuid()}";
	}
}