using Messager.NET.Requests;

namespace Messager.NET.Factories;

/// <summary>
/// Resolves synchronous request handlers from the active dependency container.
/// </summary>
public interface IRequestFactory
{
	/// <summary>
	/// Resolves a request handler with no input.
	/// </summary>
	public IRequest<TOut> Resolve<TOut>();

	/// <summary>
	/// Resolves all request handlers with no input.
	/// </summary>
	public IEnumerable<IRequest<TOut>> ResolveAll<TOut>();

	/// <summary>
	/// Resolves a request handler with input.
	/// </summary>
	public IRequest<TOut, TIn> Resolve<TOut, TIn>();

	/// <summary>
	/// Resolves all request handlers with input.
	/// </summary>
	public IEnumerable<IRequest<TOut, TIn>> ResolveAll<TOut, TIn>();

	/// <summary>
	/// Resolves a keyed request handler.
	/// </summary>
	public IRequest<TKey, TOut, TIn> Resolve<TKey, TOut, TIn>() where TKey : notnull;

	/// <summary>
	/// Resolves all keyed request handlers.
	/// </summary>
	public IEnumerable<IRequest<TKey, TOut, TIn>> ResolveAll<TKey, TOut, TIn>() where TKey : notnull;
}

/// <summary>
/// Resolves asynchronous request handlers from the active dependency container.
/// </summary>
public interface IAsyncRequestFactory
{
	/// <summary>
	/// Resolves an asynchronous request handler with no input.
	/// </summary>
	public IAsyncRequest<TOut> Resolve<TOut>();

	/// <summary>
	/// Resolves all asynchronous request handlers with no input.
	/// </summary>
	public IEnumerable<IAsyncRequest<TOut>> ResolveAll<TOut>();

	/// <summary>
	/// Resolves an asynchronous request handler with input.
	/// </summary>
	public IAsyncRequest<TOut, TIn> Resolve<TOut, TIn>();

	/// <summary>
	/// Resolves all asynchronous request handlers with input.
	/// </summary>
	public IEnumerable<IAsyncRequest<TOut, TIn>> ResolveAll<TOut, TIn>();

	/// <summary>
	/// Resolves a keyed asynchronous request handler.
	/// </summary>
	public IAsyncRequest<TKey, TOut, TIn> Resolve<TKey, TOut, TIn>() where TKey : notnull;

	/// <summary>
	/// Resolves all keyed asynchronous request handlers.
	/// </summary>
	public IEnumerable<IAsyncRequest<TKey, TOut, TIn>> ResolveAll<TKey, TOut, TIn>() where TKey : notnull;
}
