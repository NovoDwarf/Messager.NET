using Autofac;
using Messager.NET.Core;
using Messager.NET.Extensions;
using Messager.NET.Interfaces.Factories;
using Microsoft.Extensions.Logging;

namespace Messager.NET;

public sealed class MessagerModule : Module
{
	private readonly MessagerOptions _options;

	public MessagerModule(MessagerOptions? options = null)
	{
		_options = options ?? new MessagerOptions();
	}

	protected override void Load(ContainerBuilder builder)
	{
		builder.Register(c =>
			{
				var factory = _options.EnableLogging
					? c.ResolveOptional<ILoggerFactory>() 
					: null;
				
				return new Exchange(factory);
			})
			.As<IBrokerFactory>()
			.As<IKeyedBrokerFactory>()
			.SingleInstance();
		
		builder.AddPubSubRegistrations();
		builder.AddRequestRegistartions(_options.RequestAssemblies);
	}
}
