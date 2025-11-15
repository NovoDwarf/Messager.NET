namespace Messager.NET.Interfaces.Requests;

/// <summary>
/// 
/// </summary>
/// <typeparam name="TOut"></typeparam>
/// <typeparam name="TIn"></typeparam>
public interface IAsyncRequest<TOut, in TIn>
{
	/// <summary>
	/// 
	/// </summary>
	/// <param name="input"></param>
	/// <returns></returns>
	public ValueTask<TOut> InvokeAsync(TIn input);
	
	/// <summary>
	/// 
	/// </summary>
	/// <param name="input"></param>
	/// <param name="output"></param>
	/// <returns></returns>
	public bool TryInvokeAsync(TIn input, out TOut? output);

}

/// <summary>
/// 
/// </summary>
/// <typeparam name="TKey"></typeparam>
/// <typeparam name="TOut"></typeparam>
/// <typeparam name="TIn"></typeparam>
public interface IAsyncRequest<TKey, TOut, in TIn>
{
	/// <summary>
	/// 
	/// </summary>
	public TKey Key { get; }
	
	/// <summary>
	/// 
	/// </summary>
	/// <param name="key"></param>
	/// <param name="input"></param>
	/// <returns></returns>
	public ValueTask<TOut> InvokeAsync(TKey key, TIn input);
	
	/// <summary>
	/// 
	/// </summary>
	/// <param name="key"></param>
	/// <param name="input"></param>
	/// <param name="output"></param>
	/// <returns></returns>
	public bool TryInvoke(TKey key, TIn input, out TOut? output);
}