using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Messager.NET.Extensions;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddMessager(this IServiceCollection services, Action<MessagerOptions>? configureOptions = null)
	{
		var builder = new ContainerBuilder();
		
		builder.AddMessager(configureOptions);
		builder.Populate(services);
		
		return services;
	}
}