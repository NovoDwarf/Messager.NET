using System.Reflection;
using Messager.NET.Core;
using Messager.NET.Interfaces.Factories;
using Messager.NET.Interfaces.Receivers;
using Messager.NET.Interfaces.Requests;
using Messager.NET.Interfaces.Senders;
using Messager.NET.Models.Entity.PubSub.Receivers;
using Messager.NET.Models.Entity.PubSub.Senders;
using Messager.NET.Models.Factory;
using Messager.NET.Utilities.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Messager.NET.Extensions;

public static class ServiceCollectionExtensions
{


	public static IServiceCollection AddMessager(this IServiceCollection services, Action<MessagerOptions>? configureOptions = null)
	{
		var options = new MessagerOptions();

		configureOptions?.Invoke(options);

		services.AddSingleton<IBrokerFactory, Exchange>(serviceProvider =>
		{
			var factory = options is { EnableLogging: true } 
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
	
	private static void AddRequestRegistrations(IServiceCollection services, IEnumerable<Assembly> assemblies)
	{
		services.AddScoped<IRequestFactory, RequestFactory>();
		services.AddScoped<IAsyncRequestFactory, AsyncRequestFactory>();

		foreach (var assembly in assemblies.Distinct())
		{
			var requestTypes = assembly.GetTypes()
				.Where(t => t is { IsAbstract: false, IsClass: true } && ResolveHelper.IsRequestType(t));

			foreach (var type in requestTypes)
			{
				var interfaces = type.GetInterfaces().Where(ResolveHelper.IsRequestInterface);

				foreach (var interfaceType in interfaces)
				{
					services.AddScoped(interfaceType, type);
				}
			}
		}
	}



}
