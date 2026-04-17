using Messager.NET.Factories;
using Messager.NET.Requests;
using Microsoft.Extensions.DependencyInjection;

namespace Messager.NET.Microsoft.DependencyInjection.Factory;

public class RequestFactory : IRequestFactory
{
	private readonly IServiceProvider _serviceProvider;

	public RequestFactory(IServiceProvider serviceProvider)
	{
		_serviceProvider = serviceProvider;
	}

	public IRequest<TOut> Resolve<TOut>() => _serviceProvider.GetRequiredService<IRequest<TOut>>();
	public IEnumerable<IRequest<TOut>> ResolveAll<TOut>() => _serviceProvider.GetServices<IRequest<TOut>>();

	public IRequest<TOut, TIn> Resolve<TOut, TIn>() => _serviceProvider.GetRequiredService<IRequest<TOut, TIn>>();
	public IEnumerable<IRequest<TOut, TIn>> ResolveAll<TOut, TIn>() => _serviceProvider.GetServices<IRequest<TOut, TIn>>();

	public IRequest<TKey, TOut, TIn> Resolve<TKey, TOut, TIn>() where TKey : notnull => _serviceProvider.GetRequiredService<IRequest<TKey, TOut, TIn>>();
	public IEnumerable<IRequest<TKey, TOut, TIn>> ResolveAll<TKey, TOut, TIn>() where TKey : notnull => _serviceProvider.GetServices<IRequest<TKey, TOut, TIn>>();
}
