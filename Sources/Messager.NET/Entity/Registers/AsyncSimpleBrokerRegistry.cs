using Messager.NET.Entity.Brokers;
using Messager.NET.Helpers;
using Microsoft.Extensions.Logging;

namespace Messager.NET.Entity.Registers;

internal class AsyncSimpleBrokerRegistry
{
	private readonly Dictionary<Type, object> _brokers = new();
	private readonly Lock _locker = new();
	private readonly ILoggerFactory? _loggerFactory;

	public AsyncSimpleBrokerRegistry(ILoggerFactory? loggerFactory = null)
	{
		_loggerFactory = loggerFactory;
	}

	public AsyncSimpleBroker<TEvent> GetOrCreate<TEvent>()
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