using Messager.NET.Interfaces.Requests;

namespace Messager.NET.Models.Requests;

public class AsyncRequest<TOut, TIn> : IAsyncRequest<TOut, TIn>
{
	private readonly IAsyncRequest<TOut, TIn> _impl;

	public AsyncRequest(IAsyncRequest<TOut, TIn> impl)
	{
		_impl = impl;
	}

	public ValueTask<TOut> InvokeAsync(TIn input) => _impl.InvokeAsync(input);
    
	public bool TryInvokeAsync(TIn input, out TOut? output) => _impl.TryInvokeAsync(input, out output);
}

public class AsyncRequest<TKey, TOut, TIn> : IAsyncRequest<TKey, TOut, TIn>
{
	private readonly IAsyncRequest<TKey, TOut, TIn> _impl;

	public AsyncRequest(IAsyncRequest<TKey, TOut, TIn> impl)
	{
		_impl = impl;
	}

	public TKey Key => _impl.Key;
	
	public ValueTask<TOut> InvokeAsync(TKey key, TIn input) => _impl.InvokeAsync(key, input);
    
	public bool TryInvoke(TKey key, TIn input, out TOut? output) => _impl.TryInvoke(key, input, out output);
}