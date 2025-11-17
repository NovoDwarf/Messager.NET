using Autofac;
using Messager.NET.Interfaces.Factories;
using Messager.NET.Interfaces.Requests;

namespace Messager.NET.Models.Factory;

public class RequestFactory : IRequestFactory
{
	private readonly IComponentContext _context;

	public RequestFactory(IComponentContext context)
	{
		_context = context;
	}
	
	public IRequest<TOut> Resolve<TOut>() => _context.Resolve<IRequest<TOut>>();
	public IEnumerable<IRequest<TOut>> ResolveAll<TOut>() => _context.Resolve<IEnumerable<IRequest<TOut>>>();

	public IRequest<TOut, TIn> Resolve<TOut, TIn>() => _context.Resolve<IRequest<TOut, TIn>>();
	public IEnumerable<IRequest<TOut, TIn>> ResolveAll<TOut, TIn>() => _context.Resolve<IEnumerable<IRequest<TOut, TIn>>>();
	
	public IRequest<TKey, TOut, TIn> Resolve<TKey, TOut, TIn>() where TKey : notnull => _context.Resolve<IRequest<TKey, TOut, TIn>>();
	public IEnumerable<IRequest<TKey, TOut, TIn>> ResolveAll<TKey, TOut, TIn>() where TKey : notnull => _context.Resolve<IEnumerable<IRequest<TKey, TOut, TIn>>>();
}

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