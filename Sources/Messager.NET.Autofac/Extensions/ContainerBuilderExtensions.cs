using Autofac;
using Messager.NET.Autofac.Modules;

namespace Messager.NET.Autofac.Extensions;

public static class ContainerBuilderExtensions
{
	public static ContainerBuilder AddMessager(this ContainerBuilder builder, Action<MessagerOptions>? configureOptions = null)
	{
		ArgumentNullException.ThrowIfNull(builder);

		builder.RegisterModule(new MessagerModule(configureOptions));

		return builder;
	}
}
