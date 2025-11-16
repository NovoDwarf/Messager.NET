using Messager.NET.Models.Brokers;
using Messager.NET.Utilities.Helpers;
using Microsoft.Extensions.Logging;

namespace Messager.NET.Models.Registers;

internal class AsyncSimpleBrokerRegistry
{
	private readonly Dictionary<Type, object> _brokers = new();
	private readonly Lock _locker = new();
	private readonly ILoggerFactory? _loggerFactory;

	internal AsyncSimpleBrokerRegistry(ILoggerFactory? loggerFactory = null) => _loggerFactory = loggerFactory;

	internal AsyncSimpleBroker<TEvent> GetOrCreate<TEvent>()
	{
		lock (_locker)
		{
			return RegistryHelper.GetOrCreate(_brokers, typeof(TEvent), () =>
			{
				var logger = _loggerFactory?.CreateLogger<AsyncSimpleBroker<TEvent>>();
				return new AsyncSimpleBroker<TEvent>(logger);
			});
		}
	}
}