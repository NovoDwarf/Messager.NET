using System.Reflection;
using JetBrains.Annotations;
using Messager.NET.Entities;
using Messager.NET.Factories;
using Messager.NET.Microsoft.DependencyInjection.Factories;
using Messager.NET.Models.PubSub.Receivers;
using Messager.NET.Models.PubSub.Senders;
using Messager.NET.Receivers;
using Messager.NET.Senders;
using Messager.NET.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Messager.NET.Microsoft.DependencyInjection.Extensions;

/// <summary>Provides Messager.NET registration for Microsoft dependency injection.</summary>
public static class ServiceCollectionExtensions
{
	/// <summary>Registers Messager.NET services with the container.</summary>
	/// <param name="services">The container registration target.</param>
	/// <param name="configureOptions">An optional callback that configures the registrations.</param>
	/// <returns>The registration target.</returns>
	public static IServiceCollection AddMessager(this IServiceCollection services,
		[InstantHandle] Action<MessagerOptions>? configureOptions = null)
	{
		if (services.Any(x => x.ServiceType == typeof(MessagerOptions)))
			throw new InvalidOperationException("Messager.NET is already registered.");

		var options = new MessagerOptions();

		configureOptions?.Invoke(options);

		services.TryAddSingleton(options);
		services.TryAddSingleton<Exchange>(sp =>
		{
			if (options.UseGlobal)
				return Exchange.Default;

			var loggerFactory = options.EnableLogging
				? sp.GetService<ILoggerFactory>() ?? NullLoggerFactory.Instance
				: NullLoggerFactory.Instance;

			return new Exchange(loggerFactory);
		});

		services.TryAddSingleton<IBrokerFactory>(sp => sp.GetRequiredService<Exchange>());
		services.TryAddSingleton<IKeyedBrokerFactory>(sp => sp.GetRequiredService<Exchange>());

		AddPubSubRegistrations(services);
		AddRequestRegistrations(services, options.RequestAssemblies);

		return services;
	}

	private static void AddPubSubRegistrations(IServiceCollection services)
	{
		services.TryAddTransient(typeof(ISender<>), typeof(Sender<>));
		services.TryAddTransient(typeof(IReceiver<>), typeof(Receiver<>));
		services.TryAddTransient(typeof(IAsyncSender<>), typeof(AsyncSender<>));
		services.TryAddTransient(typeof(IAsyncReceiver<>), typeof(AsyncReceiver<>));
		services.TryAddTransient(typeof(ISender<,>), typeof(KeyedSender<,>));
		services.TryAddTransient(typeof(IReceiver<,>), typeof(KeyedReceiver<,>));
		services.TryAddTransient(typeof(IAsyncSender<,>), typeof(AsyncKeyedSender<,>));
		services.TryAddTransient(typeof(IAsyncReceiver<,>), typeof(AsyncKeyedReceiver<,>));
	}

	private static void AddRequestRegistrations(IServiceCollection services, IEnumerable<Assembly> assemblies)
	{
		services.TryAddScoped<IRequestFactory, RequestFactory>();
		services.TryAddScoped<IAsyncRequestFactory, AsyncRequestFactory>();

		foreach (var assembly in assemblies.Distinct())
		{
			var requestTypes = assembly
			                   .GetTypes()
			                   .Where(type =>
				                   type is { IsAbstract: false, IsClass: true } && ResolveHelper.IsRequestType(type));

			foreach (var type in requestTypes)
			{
				var interfaces = type.GetInterfaces().Where(ResolveHelper.IsRequestInterface);

				foreach (var interfaceType in interfaces)
					services.TryAddScoped(interfaceType, type);
			}
		}
	}
}