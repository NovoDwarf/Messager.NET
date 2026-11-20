using Messager.NET.Models.Brokers;
using Messager.NET.Utilities;
using Microsoft.Extensions.Logging;

namespace Messager.NET.Models.Registers;

internal class SimpleBrokerRegistry
{
	private readonly Dictionary<Type, object> _brokers = new();
	private readonly Lock _locker = new();
	
	private readonly ILoggerFactory _loggerFactory;

	internal SimpleBrokerRegistry(ILoggerFactory loggerFactory)
	{
		_loggerFactory = loggerFactory;
	}

	internal SimpleBroker<TEvent> GetOrCreate<TEvent>()
	{
		lock (_locker)
			return RegistryHelper.GetOrCreate(_brokers, typeof(TEvent), New<TEvent>);
	}

	private SimpleBroker<TEvent> New<TEvent>()
	{
		var logger = _loggerFactory.CreateLogger<SimpleBroker<TEvent>>();
		return new SimpleBroker<TEvent>(logger);
	}
}
