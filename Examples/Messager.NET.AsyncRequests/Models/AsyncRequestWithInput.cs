using Messager.NET.Requests;

namespace Messager.NET.AsyncRequests.Models;

public class AsyncRequestWithInput : IAsyncRequest<string, string>
{
	public ValueTask<string> InvokeAsync(string message)
	{
		var rev = new string(message.Reverse().ToArray());

		return new ValueTask<string>($"IAsyncRequest<string, string> return {rev}");
	}

	public bool TryInvokeAsync(string input, out string? output)
	{
		output = null;
		return true;
	}
}