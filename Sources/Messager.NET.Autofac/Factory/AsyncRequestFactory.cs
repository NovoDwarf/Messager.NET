using Autofac;
using Messager.NET.Factories;
using Messager.NET.Requests;

namespace Messager.NET.Autofac.Factory;

public class AsyncRequestFactory : IAsyncRequestFactory
{
	private readonly IComponentContext _context;

	public AsyncRequestFactory(IComponentContext context)
	{
		_context = context;
	}

	public IAsyncRequest<TOut> Resolve<TOut>() => _context.Resolve<IAsyncRequest<TOut>>();
	public IEnumerable<IAsyncRequest<TOut>> ResolveAll<TOut>() => _context.Resolve<IEnumerable<IAsyncRequest<TOut>>>();

	public IAsyncRequest<TOut, TIn> Resolve<TOut, TIn>() => _context.Resolve<IAsyncRequest<TOut, TIn>>();
	public IEnumerable<IAsyncRequest<TOut, TIn>> ResolveAll<TOut, TIn>() => _context.Resolve<IEnumerable<IAsyncRequest<TOut, TIn>>>();

	public IAsyncRequest<TKey, TOut, TIn> Resolve<TKey, TOut, TIn>() where TKey : notnull => _context.Resolve<IAsyncRequest<TKey, TOut, TIn>>();
	public IEnumerable<IAsyncRequest<TKey, TOut, TIn>> ResolveAll<TKey, TOut, TIn>() where TKey : notnull => _context.Resolve<IEnumerable<IAsyncRequest<TKey, TOut, TIn>>>();
}
