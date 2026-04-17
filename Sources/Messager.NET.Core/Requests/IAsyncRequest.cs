namespace Messager.NET.Requests;

/// <summary>
/// Represents a request with no input that returns a value asynchronously.
/// </summary>
/// <typeparam name="TOut">The response type.</typeparam>
public interface IAsyncRequest<TOut>
{
	/// <summary>
	/// Executes the request asynchronously.
	/// </summary>
	/// <returns>A task that resolves to the request result.</returns>
	public ValueTask<TOut> InvokeAsync();

	/// <summary>
	/// Attempts to produce a result synchronously without awaiting asynchronous work.
	/// </summary>
	/// <param name="output">The result when synchronous resolution succeeds.</param>
	/// <returns><see langword="true"/> when a synchronous result is available; otherwise <see langword="false"/>.</returns>
	public bool TryInvokeAsync(out TOut? output);
}

/// <summary>
/// Represents a request that accepts input and returns a value asynchronously.
/// </summary>
/// <typeparam name="TOut">The response type.</typeparam>
/// <typeparam name="TIn">The input type.</typeparam>
public interface IAsyncRequest<TOut, in TIn>
{
	/// <summary>
	/// Executes the request asynchronously with the specified <paramref name="input"/>.
	/// </summary>
	/// <param name="input">The request input.</param>
	/// <returns>A task that resolves to the request result.</returns>
	public ValueTask<TOut> InvokeAsync(TIn input);

	/// <summary>
	/// Attempts to produce a result synchronously for the specified <paramref name="input"/>.
	/// </summary>
	/// <param name="input">The request input.</param>
	/// <param name="output">The result when synchronous resolution succeeds.</param>
	/// <returns><see langword="true"/> when a synchronous result is available; otherwise <see langword="false"/>.</returns>
	public bool TryInvokeAsync(TIn input, out TOut? output);
}

/// <summary>
/// Represents a keyed request that accepts input and returns a value asynchronously.
/// </summary>
/// <typeparam name="TKey">The request key type.</typeparam>
/// <typeparam name="TOut">The response type.</typeparam>
/// <typeparam name="TIn">The input type.</typeparam>
public interface IAsyncRequest<TKey, TOut, in TIn>
	where TKey : notnull
{
	/// <summary>
	/// Gets the key associated with the request handler instance.
	/// </summary>
	public TKey Key { get; }

	/// <summary>
	/// Executes the request asynchronously for the specified <paramref name="key"/>.
	/// </summary>
	/// <param name="key">The key used to select the request handler.</param>
	/// <param name="input">The request input.</param>
	/// <returns>A task that resolves to the request result.</returns>
	public ValueTask<TOut> InvokeAsync(TKey key, TIn input);

	/// <summary>
	/// Attempts to produce a result synchronously for the specified <paramref name="key"/>.
	/// </summary>
	/// <param name="key">The key used to select the request handler.</param>
	/// <param name="input">The request input.</param>
	/// <param name="output">The result when synchronous resolution succeeds.</param>
	/// <returns><see langword="true"/> when a synchronous result is available; otherwise <see langword="false"/>.</returns>
	public bool TryInvoke(TKey key, TIn input, out TOut? output);
}
