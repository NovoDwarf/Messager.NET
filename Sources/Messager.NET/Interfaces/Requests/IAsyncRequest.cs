namespace Messager.NET.Interfaces.Requests;

public interface IAsyncRequest<TOut, TIn>
{
	public ValueTask<TOut> RequestAsync(TIn input);
	
	public bool TryRequestAsync(TIn input, out TOut? output);

}

public interface IAsyncRequest<TKey, TOut, TIn>
{
	public TKey Key { get; }
	
	public ValueTask<TOut> RequestAsync(TKey key, TIn input);
	
	public bool TryRequestAsync(TKey key, TIn input, out TOut? output);
}