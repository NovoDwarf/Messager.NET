using Messager.NET.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Messager.NET.DependencyInjection.Extensions;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddMessager(this IServiceCollection services, Action<MessagerOptions>? configureOptions = null)
	{
		var options = new MessagerOptions();

		configureOptions?.Invoke(options);

		services.AddSingleton(options);

		services.AddSingleton<IBrokerFactory, Exchange>(serviceProvider =>
		{
			var messagerOptions = serviceProvider.GetRequiredService<MessagerOptions>();
			var factory = messagerOptions.EnableLogging
				? serviceProvider.GetService<ILoggerFactory>()
				: null;

			return new Exchange(factory);
		});

		services.AddSingleton<IKeyedBrokerFactory>(serviceProvider => (Exchange)serviceProvider.GetRequiredService<IBrokerFactory>());

		AddPubSubRegistrations(services);
		AddRequestRegistrations(services, options.RequestAssemblies);

		return services;
	}

	private static void AddPubSubRegistrations(IServiceCollection services)
	{
		services.AddTransient(typeof(ISender<>), typeof(Sender<>));
		services.AddTransient(typeof(IReceiver<>), typeof(Receiver<>));
		services.AddTransient(typeof(IAsyncSender<>), typeof(AsyncSender<>));
		services.AddTransient(typeof(IAsyncReceiver<>), typeof(AsyncReceiver<>));
		services.AddTransient(typeof(ISender<,>), typeof(KeyedSender<,>));
		services.AddTransient(typeof(IReceiver<,>), typeof(KeyedReceiver<,>));
		services.AddTransient(typeof(IAsyncSender<,>), typeof(AsyncKeyedSender<,>));
		services.AddTransient(typeof(IAsyncReceiver<,>), typeof(AsyncKeyedReceiver<,>));
	}

	private static void AddRequestRegistrations(IServiceCollection services, IEnumerable<System.Reflection.Assembly> assemblies)
	{
		services.AddScoped<IRequestFactory, RequestFactory>();
		services.AddScoped<IAsyncRequestFactory, AsyncRequestFactory>();

		foreach (var assembly in assemblies.Distinct())
		{
			var requestTypes = assembly.GetTypes()
				.Where(type => type is { IsAbstract: false, IsClass: true } && ResolveHelper.IsRequestType(type));

			foreach (var type in requestTypes)
			{
				var interfaces = type.GetInterfaces().Where(ResolveHelper.IsRequestInterface);

				foreach (var interfaceType in interfaces)
					services.AddScoped(interfaceType, type);
			}
		}
	}
}
