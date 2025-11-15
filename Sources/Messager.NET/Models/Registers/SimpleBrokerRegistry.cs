using Messager.NET.Helpers;
using Messager.NET.Models.Brokers;
using Microsoft.Extensions.Logging;

namespace Messager.NET.Models.Registers;

internal class SimpleBrokerRegistry
{
	private readonly Dictionary<Type, object> _brokers = new();
	private readonly Lock _locker = new();
	
	private readonly ILoggerFactory? _loggerFactory;
	private readonly ILogger<SimpleBrokerRegistry>? _logger;
	
	public SimpleBrokerRegistry(ILoggerFactory? loggerFactory = null)
	{
		_loggerFactory = loggerFactory;
		_logger = loggerFactory?.CreateLogger<SimpleBrokerRegistry>();
	}

	public SimpleBroker<TEvent> GetOrCreate<TEvent>()
	{
		lock (_locker)
		{
			return RegistryHelper.GetOrCreate(_brokers, typeof(TEvent), () =>
			{
				var logger = _loggerFactory?.CreateLogger<SimpleBroker<TEvent>>();
				return new SimpleBroker<TEvent>(logger);
			});
		}
	}
}