using Autofac;
using Messager.NET.Autofac.Factories;
using Messager.NET.Entities;
using Messager.NET.Factories;
using Messager.NET.Models.PubSub.Receivers;
using Messager.NET.Models.PubSub.Senders;
using Messager.NET.Receivers;
using Messager.NET.Senders;
using Messager.NET.Utilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Messager.NET.Autofac.Modules;

public sealed class MessagerModule : Module
{
	private readonly MessagerOptions _options = new();

	public MessagerModule(Action<MessagerOptions>? configureOptions = null)
	{
		configureOptions?.Invoke(_options);
	}
    
	protected override void Load(ContainerBuilder builder)
	{
		builder.RegisterInstance(_options)
		       .AsSelf()
		       .SingleInstance();

		RegisterExchange(builder);
		RegisterPubSub(builder);
		RegisterRequests(builder);
	}

	private void RegisterExchange(ContainerBuilder builder)
	{
		if (_options.UseGlobal)
		{
			builder.RegisterInstance(Exchange.Default)
			       .AsSelf().As<IBrokerFactory>().As<IKeyedBrokerFactory>()
			       .ExternallyOwned();

			return;
		}

		builder.Register(RegisterNewInstance)
		       .AsSelf().As<IBrokerFactory>().As<IKeyedBrokerFactory>().SingleInstance();
	}

	private Exchange RegisterNewInstance(IComponentContext context)
	{
		var loggerFactory = _options.EnableLogging
			? context.ResolveOptional<ILoggerFactory>() ?? NullLoggerFactory.Instance
			: NullLoggerFactory.Instance;

		return new Exchange(loggerFactory);
	}

	private static void RegisterPubSub(ContainerBuilder builder)
	{
		builder.RegisterGeneric(typeof(Sender<>))
		       .As(typeof(ISender<>)).InstancePerDependency();

		builder.RegisterGeneric(typeof(Receiver<>))
		       .As(typeof(IReceiver<>)).InstancePerDependency();

		builder.RegisterGeneric(typeof(AsyncSender<>))
		       .As(typeof(IAsyncSender<>)).InstancePerDependency();

		builder.RegisterGeneric(typeof(AsyncReceiver<>))
		       .As(typeof(IAsyncReceiver<>)).InstancePerDependency();

		builder.RegisterGeneric(typeof(KeyedSender<,>))
		       .As(typeof(ISender<,>)).InstancePerDependency();

		builder.RegisterGeneric(typeof(KeyedReceiver<,>))
		       .As(typeof(IReceiver<,>)).InstancePerDependency();

		builder.RegisterGeneric(typeof(AsyncKeyedSender<,>))
		       .As(typeof(IAsyncSender<,>)).InstancePerDependency();

		builder.RegisterGeneric(typeof(AsyncKeyedReceiver<,>))
		       .As(typeof(IAsyncReceiver<,>)).InstancePerDependency();
	}

	private void RegisterRequests(ContainerBuilder builder)
	{
		builder.RegisterType<RequestFactory>().As<IRequestFactory>().InstancePerLifetimeScope();
		builder.RegisterType<AsyncRequestFactory>().As<IAsyncRequestFactory>().InstancePerLifetimeScope();

		foreach (var assembly in _options.RequestAssemblies.Distinct())
		{
			builder.RegisterAssemblyTypes(assembly)
			       .Where(type => type is { IsAbstract: false, IsClass: true } && ResolveHelper.IsRequestType(type))
			       .As(type => type.GetInterfaces().Where(ResolveHelper.IsRequestInterface))
			       .InstancePerLifetimeScope();
		}
	}
}