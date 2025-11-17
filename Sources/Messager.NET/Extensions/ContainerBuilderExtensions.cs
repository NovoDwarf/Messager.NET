using System.Reflection;
using Autofac;
using Messager.NET.Interfaces.Factories;
using Messager.NET.Interfaces.Receivers;
using Messager.NET.Interfaces.Requests;
using Messager.NET.Interfaces.Senders;
using Messager.NET.Models.Entity.PubSub.Receivers;
using Messager.NET.Models.Entity.PubSub.Senders;
using Messager.NET.Models.Factory;

namespace Messager.NET.Extensions;

public static class ContainerBuilderExtensions
{
	private static readonly HashSet<Type> RequestTypes =
	[
		typeof(IRequest<>),
		typeof(IRequest<,>),
		typeof(IRequest<,,>),
		typeof(IAsyncRequest<>),
		typeof(IAsyncRequest<,>),
		typeof(IAsyncRequest<,,>)
	];
	
	extension(ContainerBuilder builder)
	{
		public ContainerBuilder AddMessager(Action<MessagerOptions>? configureOptions = null)
		{
			var options = new MessagerOptions();
		
			configureOptions?.Invoke(options);
		
			builder.RegisterModule(new MessagerModule(options));
		
			return builder;
		}

		public ContainerBuilder AddPubSubRegistrations()
		{
			builder.RegisterGeneric(typeof(Sender<>))
				.As(typeof(ISender<>))
				.InstancePerDependency();

			builder.RegisterGeneric(typeof(Receiver<>))
				.As(typeof(IReceiver<>))
				.InstancePerDependency();

			builder.RegisterGeneric(typeof(KeyedSender<,>))
				.As(typeof(ISender<,>))
				.InstancePerDependency();

			builder.RegisterGeneric(typeof(KeyedReceiver<,>))
				.As(typeof(IReceiver<,>))
				.InstancePerDependency();
			
			return builder;
		}

		public ContainerBuilder AddRequestRegistartions()
		{
			var userAssemblies = new List<Assembly?>() { Assembly.GetEntryAssembly() };
			
			builder.RegisterType<RequestFactory>().As<IRequestFactory>().InstancePerLifetimeScope();
			builder.RegisterType<AsyncRequestFactory>().As<IAsyncRequestFactory>().InstancePerLifetimeScope();

			foreach (var assembly in userAssemblies.OfType<Assembly>())
			{
				builder.RegisterAssemblyTypes(assembly)
					.Where(t => t is { IsAbstract: false, IsClass: true } && IsRequestType(t))
					.AsImplementedInterfaces()
					.InstancePerLifetimeScope();
			}
			
			return builder;
		}
		
		private static bool IsRequestType(Type type)
		{
			return type.GetInterfaces()
				.Where(i => i.IsGenericType)
				.Select(i => i.GetGenericTypeDefinition())
				.Any(genericType => RequestTypes.Contains(genericType));
		}
	}
}