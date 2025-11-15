using Messager.NET.Interfaces.Factories;
using Messager.NET.Interfaces.Receivers;
using Messager.NET.Interfaces.Senders;
using Messager.NET.Models.Registers;
using Microsoft.Extensions.Logging;

namespace Messager.NET.Core;

/// <summary>
/// 
/// </summary>
public sealed class Exchange : IMessageBrokerFactory, IKeyedMessageBrokerFactory
{
    private readonly SimpleBrokerRegistry _simpleBrokers;
    private readonly KeyedBrokerRegistry _keyedBrokers;
    private readonly AsyncSimpleBrokerRegistry _asyncSimpleBrokers;
    private readonly AsyncKeyedBrokerRegistry _asyncKeyedBrokers;
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="loggerFactory"></param>
    public Exchange(ILoggerFactory? loggerFactory = null)
    {
        _simpleBrokers = new SimpleBrokerRegistry(loggerFactory);
        _keyedBrokers = new KeyedBrokerRegistry(loggerFactory);
        _asyncSimpleBrokers = new AsyncSimpleBrokerRegistry(loggerFactory);
        _asyncKeyedBrokers = new AsyncKeyedBrokerRegistry(loggerFactory);
    }
    
    /// <inheritdoc/>
    public ISender<TEvent> GetSender<TEvent>() => _simpleBrokers.GetOrCreate<TEvent>();
    
    /// <inheritdoc/>
    public IReceiver<TEvent> GetReceiver<TEvent>() => _simpleBrokers.GetOrCreate<TEvent>();
    
    /// <inheritdoc/>
    public IAsyncSender<TEvent> GetAsyncSender<TEvent>() => _asyncSimpleBrokers.GetOrCreate<TEvent>();
    
    /// <inheritdoc/>
    public IAsyncReceiver<TEvent> GetAsyncReceiver<TEvent>() => _asyncSimpleBrokers.GetOrCreate<TEvent>();
    
    /// <inheritdoc/>
    public ISender<TKey, TEvent> GetKeyedSender<TKey, TEvent>() where TKey : notnull => _keyedBrokers.GetOrCreate<TKey, TEvent>();
    
    /// <inheritdoc/>
    public IReceiver<TKey, TEvent> GetKeyedReceiver<TKey, TEvent>() where TKey : notnull => _keyedBrokers.GetOrCreate<TKey, TEvent>();
    
    /// <inheritdoc/>
    public IAsyncSender<TKey, TEvent> GetAsyncKeyedSender<TKey, TEvent>() where TKey : notnull => _asyncKeyedBrokers.GetOrCreate<TKey, TEvent>();
    
    /// <inheritdoc/>
    public IAsyncReceiver<TKey, TEvent> GetAsyncKeyedReceiver<TKey, TEvent>() where TKey : notnull => _asyncKeyedBrokers.GetOrCreate<TKey, TEvent>();
}