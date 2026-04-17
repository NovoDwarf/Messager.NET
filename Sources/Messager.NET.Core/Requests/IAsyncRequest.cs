namespace Messager.NET.Requests;

public interface IAsyncRequest<TOut>
{
	public ValueTask<TOut> InvokeAsync();

	public bool TryInvokeAsync(out TOut? output);
}

public interface IAsyncRequest<TOut, in TIn>
{
	public ValueTask<TOut> InvokeAsync(TIn input);

	public bool TryInvokeAsync(TIn input, out TOut? output);
}

public interface IAsyncRequest<TKey, TOut, in TIn>
	where TKey : notnull
{
	public TKey Key { get; }
	
	public ValueTask<TOut> InvokeAsync(TKey key, TIn input);
	
	public bool TryInvoke(TKey key, TIn input, out TOut? output);
}