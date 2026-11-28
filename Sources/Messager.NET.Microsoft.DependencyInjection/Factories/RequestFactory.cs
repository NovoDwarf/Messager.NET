using Messager.NET.Factories;
using Messager.NET.Requests;
using Microsoft.Extensions.DependencyInjection;

namespace Messager.NET.Microsoft.DependencyInjection.Factories;

/// <summary>Resolves request handlers from the dependency injection container.</summary>
public class RequestFactory : IRequestFactory
{
	private readonly IServiceProvider _serviceProvider;

	/// <summary>Creates a request factory using the specified container context.</summary>
	public RequestFactory(IServiceProvider serviceProvider)
	{
		_serviceProvider = serviceProvider;
	}

	/// <inheritdoc />
	public IRequest<TOut> Resolve<TOut>() => _serviceProvider.GetRequiredService<IRequest<TOut>>();
	/// <inheritdoc />
	public IEnumerable<IRequest<TOut>> ResolveAll<TOut>() => _serviceProvider.GetServices<IRequest<TOut>>();

	/// <inheritdoc />
	public IRequest<TOut, TIn> Resolve<TOut, TIn>() => _serviceProvider.GetRequiredService<IRequest<TOut, TIn>>();
	/// <inheritdoc />
	public IEnumerable<IRequest<TOut, TIn>> ResolveAll<TOut, TIn>() => _serviceProvider.GetServices<IRequest<TOut, TIn>>();

	/// <inheritdoc />
	public IRequest<TKey, TOut, TIn> Resolve<TKey, TOut, TIn>() where TKey : notnull => _serviceProvider.GetRequiredService<IRequest<TKey, TOut, TIn>>();
	/// <inheritdoc />
	public IEnumerable<IRequest<TKey, TOut, TIn>> ResolveAll<TKey, TOut, TIn>() where TKey : notnull => _serviceProvider.GetServices<IRequest<TKey, TOut, TIn>>();
}
