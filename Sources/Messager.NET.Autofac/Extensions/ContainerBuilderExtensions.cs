using Autofac;
using JetBrains.Annotations;
using Messager.NET.Autofac.Modules;

namespace Messager.NET.Autofac.Extensions;

/// <summary>Provides Messager.NET registration for Autofac.</summary>
public static class ContainerBuilderExtensions
{
	/// <summary>Registers Messager.NET services with the container.</summary>
	/// <param name="builder">The container registration target.</param>
	/// <param name="configureOptions">An optional callback that configures the registrations.</param>
	/// <returns>The registration target.</returns>
	public static ContainerBuilder AddMessager(this ContainerBuilder builder,
		[InstantHandle] Action<MessagerOptions>? configureOptions = null)
	{
		ArgumentNullException.ThrowIfNull(builder);

		builder.RegisterModule(new MessagerModule(configureOptions));

		return builder;
	}
}