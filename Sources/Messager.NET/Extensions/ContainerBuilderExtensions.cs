using Autofac;
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

public static class ContainerBuilderExtensions
{
	public static ContainerBuilder AddMessager(this ContainerBuilder builder, Action<MessagerOptions>? configureOptions = null)
	{
		var options = new MessagerOptions();
		
		configureOptions?.Invoke(options);
		
		builder.RegisterModule(new MessagerModule(options));
		
		return builder;
	}
}