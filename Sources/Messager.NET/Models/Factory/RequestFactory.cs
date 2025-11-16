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
	
	public IRequest<TOut> Resolve<TOut>()
	{
		return _context.Resolve<IRequest<TOut>>();
	}

	public IRequest<TOut, TIn> Resolve<TOut, TIn>()
	{
		return _context.Resolve<IRequest<TOut, TIn>>();
	}
	
	public IEnumerable<IRequest<TOutput>> ResolveAll<TOutput>()
	{
		return _context.Resolve<IEnumerable<IRequest<TOutput>>>();
	}
	
	public IEnumerable<IRequest<TOut, TIn>> ResolveAll<TOut, TIn>()
	{
		return _context.Resolve<IEnumerable<IRequest<TOut, TIn>>>();
	}
}