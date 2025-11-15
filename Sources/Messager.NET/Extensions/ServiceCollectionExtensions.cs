using Microsoft.Extensions.DependencyInjection;

namespace Messager.NET.Extensions;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddMessager(this IServiceCollection services, Action<MessagerOptions>? configureOptions = null)
	{
		// TODO: add injection method for Microsoft.Extensions.DependencyInjection;
		
		return services;
	}
}