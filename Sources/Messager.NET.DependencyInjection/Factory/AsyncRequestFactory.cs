using Messager.NET.Factories;
using Messager.NET.Requests;
using Microsoft.Extensions.DependencyInjection;

namespace Messager.NET.DependencyInjection.Factory;

public class AsyncRequestFactory : IAsyncRequestFactory
{
	private readonly IServiceProvider _serviceProvider;

	public AsyncRequestFactory(IServiceProvider serviceProvider)
	{
		_serviceProvider = serviceProvider;
	}

	public IAsyncRequest<TOut> Resolve<TOut>() => _serviceProvider.GetRequiredService<IAsyncRequest<TOut>>();
	public IEnumerable<IAsyncRequest<TOut>> ResolveAll<TOut>() => _serviceProvider.GetServices<IAsyncRequest<TOut>>();

	public IAsyncRequest<TOut, TIn> Resolve<TOut, TIn>() => _serviceProvider.GetRequiredService<IAsyncRequest<TOut, TIn>>();
	public IEnumerable<IAsyncRequest<TOut, TIn>> ResolveAll<TOut, TIn>() => _serviceProvider.GetServices<IAsyncRequest<TOut, TIn>>();

	public IAsyncRequest<TKey, TOut, TIn> Resolve<TKey, TOut, TIn>() where TKey : notnull => _serviceProvider.GetRequiredService<IAsyncRequest<TKey, TOut, TIn>>();
	public IEnumerable<IAsyncRequest<TKey, TOut, TIn>> ResolveAll<TKey, TOut, TIn>() where TKey : notnull => _serviceProvider.GetServices<IAsyncRequest<TKey, TOut, TIn>>();
}
