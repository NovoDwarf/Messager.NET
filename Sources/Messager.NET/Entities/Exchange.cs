using Messager.NET.Factories;
using Messager.NET.Models.Registers;
using Messager.NET.Receivers;
using Messager.NET.Senders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Messager.NET.Entities;

public sealed class Exchange : IBrokerFactory, IKeyedBrokerFactory
{
	private static readonly Lazy<Exchange> GlobalInstance =
		new(() => new Exchange(), LazyThreadSafetyMode.ExecutionAndPublication);

	private readonly SimpleBrokerRegistry _simpleBrokers;
	private readonly KeyedBrokerRegistry _keyedBrokers;

	private readonly AsyncSimpleBrokerRegistry _asyncSimpleBrokers;
	private readonly AsyncKeyedBrokerRegistry _asyncKeyedBrokers;

	public Exchange() : this(NullLoggerFactory.Instance)
	{
	}

	public Exchange(ILoggerFactory? loggerFactory)
	{
		loggerFactory ??= NullLoggerFactory.Instance;

		_simpleBrokers = new SimpleBrokerRegistry(loggerFactory);
		_keyedBrokers = new KeyedBrokerRegistry(loggerFactory);

		_asyncSimpleBrokers = new AsyncSimpleBrokerRegistry(loggerFactory);
		_asyncKeyedBrokers = new AsyncKeyedBrokerRegistry(loggerFactory);
	}

	public static Exchange Default => GlobalInstance.Value;

	public ISender<TEvent> GetSender<TEvent>()
	{
		return _simpleBrokers.GetOrCreate<TEvent>();
	}

	public IReceiver<TEvent> GetReceiver<TEvent>()
	{
		return _simpleBrokers.GetOrCreate<TEvent>();
	}

	public IAsyncSender<TEvent> GetAsyncSender<TEvent>()
	{
		return _asyncSimpleBrokers.GetOrCreate<TEvent>();
	}

	public IAsyncReceiver<TEvent> GetAsyncReceiver<TEvent>()
	{
		return _asyncSimpleBrokers.GetOrCreate<TEvent>();
	}

	public ISender<TKey, TEvent> GetKeyedSender<TKey, TEvent>() where TKey : notnull
	{
		return _keyedBrokers.GetOrCreate<TKey, TEvent>();
	}

	public IReceiver<TKey, TEvent> GetKeyedReceiver<TKey, TEvent>() where TKey : notnull
	{
		return _keyedBrokers.GetOrCreate<TKey, TEvent>();
	}

	public IAsyncSender<TKey, TEvent> GetAsyncKeyedSender<TKey, TEvent>() where TKey : notnull
	{
		return _asyncKeyedBrokers.GetOrCreate<TKey, TEvent>();
	}

	public IAsyncReceiver<TKey, TEvent> GetAsyncKeyedReceiver<TKey, TEvent>() where TKey : notnull
	{
		return _asyncKeyedBrokers.GetOrCreate<TKey, TEvent>();
	}
}