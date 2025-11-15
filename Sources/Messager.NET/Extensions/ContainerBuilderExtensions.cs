using Autofac;

namespace Messager.NET.Extensions;

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