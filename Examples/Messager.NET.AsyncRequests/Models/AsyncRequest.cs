using Messager.NET.Requests;

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