using Messager.NET.Factories;
using Messager.NET.Requests;
using Microsoft.Extensions.DependencyInjection;

namespace Messager.NET.Microsoft.DependencyInjection.Factories;

/// <summary>Resolves asynchronous request handlers from the dependency injection container.</summary>
public class AsyncRequestFactory : IAsyncRequestFactory
{
	private readonly IServiceProvider _serviceProvider;

	/// <summary>Creates a request factory using the specified container context.</summary>
	public AsyncRequestFactory(IServiceProvider serviceProvider)
	{
		_serviceProvider = serviceProvider;
	}

	/// <inheritdoc />
	public IAsyncRequest<TOut> Resolve<TOut>() => _serviceProvider.GetRequiredService<IAsyncRequest<TOut>>();
	/// <inheritdoc />
	public IEnumerable<IAsyncRequest<TOut>> ResolveAll<TOut>() => _serviceProvider.GetServices<IAsyncRequest<TOut>>();

	/// <inheritdoc />
	public IAsyncRequest<TOut, TIn> Resolve<TOut, TIn>() => _serviceProvider.GetRequiredService<IAsyncRequest<TOut, TIn>>();
	/// <inheritdoc />
	public IEnumerable<IAsyncRequest<TOut, TIn>> ResolveAll<TOut, TIn>() => _serviceProvider.GetServices<IAsyncRequest<TOut, TIn>>();

	/// <inheritdoc />
	public IAsyncRequest<TKey, TOut, TIn> Resolve<TKey, TOut, TIn>() where TKey : notnull => _serviceProvider.GetRequiredService<IAsyncRequest<TKey, TOut, TIn>>();
	/// <inheritdoc />
	public IEnumerable<IAsyncRequest<TKey, TOut, TIn>> ResolveAll<TKey, TOut, TIn>() where TKey : notnull => _serviceProvider.GetServices<IAsyncRequest<TKey, TOut, TIn>>();
}
