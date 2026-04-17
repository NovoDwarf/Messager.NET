using System.Reflection;
using Autofac;
using Messager.NET.Autofac.Factory;
using Messager.NET.Entities;
using Messager.NET.Factories;
using Messager.NET.Models.PubSub.Receivers;
using Messager.NET.Models.PubSub.Senders;
using Messager.NET.Receivers;
using Messager.NET.Senders;
using Messager.NET.Utilities;
using Microsoft.Extensions.Logging;

namespace Messager.NET.Autofac.Extensions;

public static class ContainerBuilderExtensions
{
	public static ContainerBuilder AddMessager(this ContainerBuilder builder, Action<MessagerOptions>? configureOptions = null)
	{
		var options = new MessagerOptions();

		configureOptions?.Invoke(options);

		builder.RegisterInstance(options)
			.AsSelf()
			.SingleInstance();

		builder.Register(context =>
			{
				var registeredOptions = context.Resolve<MessagerOptions>();
				var loggerFactory = registeredOptions.EnableLogging
					? context.ResolveOptional<ILoggerFactory>()
					: null;

				return new Exchange(loggerFactory);
			})
			.As<IBrokerFactory>()
			.As<IKeyedBrokerFactory>()
			.SingleInstance();

		AddPubSubRegistrations(builder);
		AddRequestRegistrations(builder, options.RequestAssemblies);

		return builder;
	}

	private static void AddPubSubRegistrations(ContainerBuilder builder)
	{
		builder.RegisterGeneric(typeof(Sender<>))
			.As(typeof(ISender<>))
			.InstancePerDependency();

		builder.RegisterGeneric(typeof(Receiver<>))
			.As(typeof(IReceiver<>))
			.InstancePerDependency();

		builder.RegisterGeneric(typeof(AsyncSender<>))
			.As(typeof(IAsyncSender<>))
			.InstancePerDependency();

		builder.RegisterGeneric(typeof(AsyncReceiver<>))
			.As(typeof(IAsyncReceiver<>))
			.InstancePerDependency();

		builder.RegisterGeneric(typeof(KeyedSender<,>))
			.As(typeof(ISender<,>))
			.InstancePerDependency();

		builder.RegisterGeneric(typeof(KeyedReceiver<,>))
			.As(typeof(IReceiver<,>))
			.InstancePerDependency();

		builder.RegisterGeneric(typeof(AsyncKeyedSender<,>))
			.As(typeof(IAsyncSender<,>))
			.InstancePerDependency();

		builder.RegisterGeneric(typeof(AsyncKeyedReceiver<,>))
			.As(typeof(IAsyncReceiver<,>))
			.InstancePerDependency();
	}

	private static void AddRequestRegistrations(ContainerBuilder builder, IEnumerable<Assembly> assemblies)
	{
		builder.RegisterType<RequestFactory>().As<IRequestFactory>().InstancePerLifetimeScope();
		builder.RegisterType<AsyncRequestFactory>().As<IAsyncRequestFactory>().InstancePerLifetimeScope();

		foreach (var assembly in assemblies.Distinct())
		{
			builder.RegisterAssemblyTypes(assembly)
				.Where(type => type is { IsAbstract: false, IsClass: true } && ResolveHelper.IsRequestType(type))
				.As(type => type.GetInterfaces().Where(ResolveHelper.IsRequestInterface))
				.InstancePerLifetimeScope();
		}
	}
}
