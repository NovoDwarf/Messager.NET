using Autofac;
using Messager.NET.Factories;
using Messager.NET.Requests;

namespace Messager.NET.Autofac.Factories;

/// <summary>Resolves asynchronous request handlers from the dependency injection container.</summary>
public class AsyncRequestFactory : IAsyncRequestFactory
{
	private readonly IComponentContext _context;

	/// <summary>Creates a request factory using the specified container context.</summary>
	public AsyncRequestFactory(IComponentContext context)
	{
		_context = context;
	}

	/// <inheritdoc />
	public IAsyncRequest<TOut> Resolve<TOut>() => _context.Resolve<IAsyncRequest<TOut>>();
	/// <inheritdoc />
	public IEnumerable<IAsyncRequest<TOut>> ResolveAll<TOut>() => _context.Resolve<IEnumerable<IAsyncRequest<TOut>>>();

	/// <inheritdoc />
	public IAsyncRequest<TOut, TIn> Resolve<TOut, TIn>() => _context.Resolve<IAsyncRequest<TOut, TIn>>();
	/// <inheritdoc />
	public IEnumerable<IAsyncRequest<TOut, TIn>> ResolveAll<TOut, TIn>() => _context.Resolve<IEnumerable<IAsyncRequest<TOut, TIn>>>();

	/// <inheritdoc />
	public IAsyncRequest<TKey, TOut, TIn> Resolve<TKey, TOut, TIn>() where TKey : notnull => _context.Resolve<IAsyncRequest<TKey, TOut, TIn>>();
	/// <inheritdoc />
	public IEnumerable<IAsyncRequest<TKey, TOut, TIn>> ResolveAll<TKey, TOut, TIn>() where TKey : notnull => _context.Resolve<IEnumerable<IAsyncRequest<TKey, TOut, TIn>>>();
}
