using Autofac;
using Messager.NET.Factories;
using Messager.NET.Requests;

namespace Messager.NET.Autofac.Factory;

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
