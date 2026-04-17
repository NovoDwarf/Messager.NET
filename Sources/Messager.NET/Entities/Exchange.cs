using Messager.NET.Factories;
using Messager.NET.Models.Registers;
using Messager.NET.Receivers;
using Messager.NET.Senders;
using Microsoft.Extensions.Logging;

namespace Messager.NET.Entities;

public sealed class Exchange : IBrokerFactory, IKeyedBrokerFactory
{
    private readonly SimpleBrokerRegistry _simpleBrokers;
    private readonly KeyedBrokerRegistry _keyedBrokers;
    
    private readonly AsyncSimpleBrokerRegistry _asyncSimpleBrokers;
    private readonly AsyncKeyedBrokerRegistry _asyncKeyedBrokers;
    
    public Exchange(ILoggerFactory? loggerFactory = null)
    {
        _simpleBrokers = new SimpleBrokerRegistry(loggerFactory);
        _keyedBrokers = new KeyedBrokerRegistry(loggerFactory);
        
        _asyncSimpleBrokers = new AsyncSimpleBrokerRegistry(loggerFactory);
        _asyncKeyedBrokers = new AsyncKeyedBrokerRegistry(loggerFactory);
    }

    public ISender<TEvent> GetSender<TEvent>() => _simpleBrokers.GetOrCreate<TEvent>();
    public IReceiver<TEvent> GetReceiver<TEvent>() => _simpleBrokers.GetOrCreate<TEvent>();

    public IAsyncSender<TEvent> GetAsyncSender<TEvent>() => _asyncSimpleBrokers.GetOrCreate<TEvent>();
    public IAsyncReceiver<TEvent> GetAsyncReceiver<TEvent>() => _asyncSimpleBrokers.GetOrCreate<TEvent>();
    
    public ISender<TKey, TEvent> GetKeyedSender<TKey, TEvent>() where TKey : notnull => _keyedBrokers.GetOrCreate<TKey, TEvent>();
    public IReceiver<TKey, TEvent> GetKeyedReceiver<TKey, TEvent>() where TKey : notnull => _keyedBrokers.GetOrCreate<TKey, TEvent>();
    
    public IAsyncSender<TKey, TEvent> GetAsyncKeyedSender<TKey, TEvent>() where TKey : notnull => _asyncKeyedBrokers.GetOrCreate<TKey, TEvent>();
    public IAsyncReceiver<TKey, TEvent> GetAsyncKeyedReceiver<TKey, TEvent>() where TKey : notnull => _asyncKeyedBrokers.GetOrCreate<TKey, TEvent>();
}