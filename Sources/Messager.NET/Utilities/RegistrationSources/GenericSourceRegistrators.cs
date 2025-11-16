using Autofac.Builder;
using Autofac.Core;

namespace Messager.NET.Utilities.RegistrationSources;

internal class GenericRegistrationSource : IRegistrationSource
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