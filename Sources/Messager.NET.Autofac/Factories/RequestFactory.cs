using Autofac;
using Messager.NET.Factories;
using Messager.NET.Requests;

namespace Messager.NET.Autofac.Factories;

/// <summary>Resolves request handlers from the dependency injection container.</summary>
public class RequestFactory : IRequestFactory
{
	private readonly IComponentContext _context;

	/// <summary>Creates a request factory using the specified container context.</summary>
	public RequestFactory(IComponentContext context)
	{
		_context = context;
	}

	/// <inheritdoc />
	public IRequest<TOut> Resolve<TOut>() => _context.Resolve<IRequest<TOut>>();
	/// <inheritdoc />
	public IEnumerable<IRequest<TOut>> ResolveAll<TOut>() => _context.Resolve<IEnumerable<IRequest<TOut>>>();

	/// <inheritdoc />
	public IRequest<TOut, TIn> Resolve<TOut, TIn>() => _context.Resolve<IRequest<TOut, TIn>>();
	/// <inheritdoc />
	public IEnumerable<IRequest<TOut, TIn>> ResolveAll<TOut, TIn>() => _context.Resolve<IEnumerable<IRequest<TOut, TIn>>>();

	/// <inheritdoc />
	public IRequest<TKey, TOut, TIn> Resolve<TKey, TOut, TIn>() where TKey : notnull => _context.Resolve<IRequest<TKey, TOut, TIn>>();
	/// <inheritdoc />
	public IEnumerable<IRequest<TKey, TOut, TIn>> ResolveAll<TKey, TOut, TIn>() where TKey : notnull => _context.Resolve<IEnumerable<IRequest<TKey, TOut, TIn>>>();
}
