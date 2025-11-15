using Autofac;
using Autofac.Builder;
using Autofac.Core;
using Messager.NET.Core;
using Messager.NET.Interfaces.Factories;
using Messager.NET.Interfaces.Receivers;
using Messager.NET.Interfaces.Requests;
using Messager.NET.Interfaces.Senders;
using Messager.NET.Models.Receivers;
using Messager.NET.Models.Requests;
using Messager.NET.Models.Senders;
using Microsoft.Extensions.Logging;

namespace Messager.NET;

public sealed class MessagerModule : Module
{
	private readonly MessagerOptions? _options;

	public MessagerModule(MessagerOptions? options = null)
	{
		_options = options ?? new MessagerOptions();
	}

	/// <inheritdoc/>
	protected override void Load(ContainerBuilder builder)
	{
		builder.Register(c =>
			{
				var factory = _options is { EnableLogging: true } 
					? c.Resolve<ILoggerFactory>() 
					: null;
				
				return new Exchange(factory);
			})
			.As<IMessageBrokerFactory>()
			.As<IKeyedMessageBrokerFactory>()
			.SingleInstance();

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

		builder.RegisterGeneric(typeof(Request<,>))
			.As(typeof(IRequest<,>));
		
		builder.RegisterGeneric(typeof(AsyncRequest<,>))
			.As(typeof(IAsyncRequest<,>));
		
		builder.RegisterSource(new GenericRegistrationSource(
			serviceType => serviceType == typeof(ISender<,>),
			typeArgs => typeof(KeyedSender<,>).MakeGenericType(typeArgs)
		));

		builder.RegisterSource(new GenericRegistrationSource(
			serviceType => serviceType == typeof(IReceiver<,>),
			typeArgs => typeof(KeyedReceiver<,>).MakeGenericType(typeArgs)
		));

		builder.RegisterSource(new GenericRegistrationSource(
			serviceType => serviceType == typeof(IRequest<,>),
			typeArgs => typeof(Request<,>).MakeGenericType(typeArgs)
		));

		builder.RegisterSource(new GenericRegistrationSource(
			serviceType => serviceType == typeof(IAsyncRequest<,>),
			typeArgs => typeof(AsyncRequest<,>).MakeGenericType(typeArgs)
		));
	}

	private class GenericRegistrationSource : IRegistrationSource
	{
		private readonly Func<Type, bool> _serviceFilter;
		private readonly Func<Type[], Type> _concreteTypeFactory;
    
		public GenericRegistrationSource(
			Func<Type, bool> serviceFilter,
			Func<Type[], Type> concreteTypeFactory)
		{
			_serviceFilter = serviceFilter;
			_concreteTypeFactory = concreteTypeFactory;
		}

		public bool IsAdapterForIndividualComponents => false;

		public IEnumerable<IComponentRegistration> RegistrationsFor(
			Service service, 
			Func<Service, IEnumerable<ServiceRegistration>> registrationAccessor)
		{
			if (service is not IServiceWithType swt || !swt.ServiceType.IsGenericType)
				yield break;
        
			if (!_serviceFilter(swt.ServiceType.GetGenericTypeDefinition()))
				yield break;
        
			var args = swt.ServiceType.GetGenericArguments();
			var concreteType = _concreteTypeFactory(args);
			var registration = RegistrationBuilder
				.ForType(concreteType)
				.As(service)
				.InstancePerDependency()
				.CreateRegistration();
        
			yield return registration;
		}
	}
}


