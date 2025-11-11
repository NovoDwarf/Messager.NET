using Microsoft.Extensions.Logging;

namespace Messager.NET.Extensions;

public static partial class LoggerExtensions
{
    #region Simple

    [LoggerMessage(LogLevel.Debug, "[{BrokerType}<{EventType}> [{Id}]] Subscriber added")]
    public static partial void LogSubscriberAdded(this ILogger logger, string brokerType, string eventType, Guid id);
    
    [LoggerMessage(LogLevel.Debug, "[{BrokerType}<{EventType}> [{Id}]] Subscriber removed")]
    public static partial void LogSubscriberRemoved(this ILogger logger, string brokerType, string eventType, Guid id);
    
    [LoggerMessage(LogLevel.Debug, "[{BrokerType}<{EventType}> [{Id}]] Subsrcibers removed [{Count}]")]
    public static partial void LogSubscribersRemoved(this ILogger logger, string brokerType, string eventType, Guid id, int count);

    [LoggerMessage(LogLevel.Error, "[{BrokerType}<{EventType}> [{Id}]] Error invoking handler for event")]
    public static partial void LogErrorInvokingHandler(this ILogger logger, Exception ex, string brokerType, string eventType, Guid id);
    #endregion

    #region Keyed

    [LoggerMessage(LogLevel.Debug, "[{BrokerType}<{KeyType}, {EventType}> [{Id}]] Subscriber added for key [{Key}]")]
    public static partial void LogSubscriberAddedForKey(this ILogger logger, string brokerType, string keyType, string eventType, Guid id, string key);

    [LoggerMessage(LogLevel.Debug, "[{BrokerType}<{KeyType}, {EventType}> [{Id}]] Subscriber removed for key [{Key}]")]
    public static partial void LogSubscriberRemovedForKey(this ILogger logger, string brokerType, string keyType, string eventType, Guid id, string key);

    [LoggerMessage(LogLevel.Debug, "[{BrokerType}<{KeyType}, {EventType}> [{Id}]] Subscribers removed for key [{Key}] [{Count}]")]
    public static partial void LogSubscribersRemovedForKey(this ILogger logger, string brokerType, string keyType, string eventType, Guid id, string key, int count);
    
    [LoggerMessage(LogLevel.Debug, "[{BrokerType}<{KeyType}, {EventType}> [{Id}]] Created new subscription list for key [{Key}]")]
    public static partial void LogCreateSubscriptionListForKey(this ILogger logger, string brokerType, string keyType, string eventType, Guid id, string key);

    [LoggerMessage(LogLevel.Warning, "[{BrokerType}<{KeyType}, {EventType}> [{Id}]] Key [{Key}] not found in subscriptions")]
    public static partial void LogKeyNotFound(this ILogger logger, string brokerType, string keyType, string eventType, Guid id, string key);

    [LoggerMessage(LogLevel.Debug, "[{BrokerType}<{KeyType}, {EventType}> [{Id}]] Removed empty key [{Key}]")]
    public static partial void LogRemovedEmptyKey(this ILogger logger, string brokerType, string keyType, string eventType, Guid id, string key);

    [LoggerMessage(LogLevel.Error, "[{BrokerType}<{KeyType}, {EventType}> [{Id}]] Error invoking handler for event")]
    public static partial void LogErrorInvokingHandlerForKey(this ILogger logger, Exception ex, string brokerType, string keyType, string eventType, Guid id);


    #endregion
}