using Messager.NET.Interfaces.Requests;

namespace Messager.NET.AsyncRequests.Models;

public class AsyncRequest : IAsyncRequest<string>
{
	public ValueTask<string> InvokeAsync()
	{
		return new ValueTask<string>($"IAsyncRequest<string> return {Guid.NewGuid()}");
	}

	public bool TryInvokeAsync(out string? output)
	{
		output = null;
		return true;
	}
}

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