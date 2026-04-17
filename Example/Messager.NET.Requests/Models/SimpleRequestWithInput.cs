namespace Messager.NET.Requests.Models;

public class SimpleRequestWithInput : IRequest<string, string>
{
	public string Invoke(string input)
	{
		var rev = new string(input.Reverse().ToArray());
		
		return $"IRequest<string, string> return {rev}";
	}
}