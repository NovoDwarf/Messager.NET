namespace Messager.NET.Requests;

/// <summary>
/// Represents a request with no input that returns a value synchronously.
/// </summary>
/// <typeparam name="TOut">The response type.</typeparam>
public interface IRequest<out TOut>
{
	/// <summary>
	/// Executes the request.
	/// </summary>
	/// <returns>The request result.</returns>
	public TOut Invoke();
}

/// <summary>
/// Represents a request that accepts input and returns a value synchronously.
/// </summary>
/// <typeparam name="TOut">The response type.</typeparam>
/// <typeparam name="TIn">The input type.</typeparam>
public interface IRequest<out TOut, in TIn>
{
	/// <summary>
	/// Executes the request with the specified <paramref name="input"/>.
	/// </summary>
	/// <param name="input">The request input.</param>
	/// <returns>The request result.</returns>
	public TOut Invoke(TIn input);
}

/// <summary>
/// Represents a keyed request that accepts input and returns a value synchronously.
/// </summary>
/// <typeparam name="TKey">The request key type.</typeparam>
/// <typeparam name="TOut">The response type.</typeparam>
/// <typeparam name="TIn">The input type.</typeparam>
public interface IRequest<in TKey, out TOut, in TIn>
	where TKey : notnull
{
	/// <summary>
	/// Executes the request for the specified <paramref name="key"/>.
	/// </summary>
	/// <param name="key">The key used to select the request handler.</param>
	/// <param name="input">The request input.</param>
	/// <returns>The request result.</returns>
	public TOut Invoke(TKey key, TIn input);
}
