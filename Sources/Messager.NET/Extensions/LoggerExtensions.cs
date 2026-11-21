using Microsoft.Extensions.Logging;

namespace Messager.NET.Extensions;

internal static partial class LoggerExtensions
{
    // ---------------------------------------------
    // Simple subscriptions: 1000-1099
    // ---------------------------------------------
    
    [LoggerMessage(EventId = 1000, Level = LogLevel.Debug, Message = "[{BrokerType}<{EventType}> | {BrokerId}] Subscriber added")]
    internal static partial void LogSubscriberAdded(this ILogger logger, string brokerType, string eventType, Guid brokerId);

    [LoggerMessage(EventId = 1001, Level = LogLevel.Debug, Message = "[{BrokerType}<{EventType}> | {BrokerId}] Subscriber removed")]
    internal static partial void LogSubscriberRemoved(this ILogger logger, string brokerType, string eventType, Guid brokerId);

    [LoggerMessage(EventId = 1002, Level = LogLevel.Debug, Message = "[{BrokerType}<{EventType}> | {BrokerId}] Removed {Count} subscribers")]
    internal static partial void LogSubscribersRemoved(this ILogger logger, string brokerType, string eventType, Guid brokerId, int count);

    [LoggerMessage(EventId = 1003, Level = LogLevel.Error, Message = "[{BrokerType}<{EventType}> | {BrokerId}] Handler invocation failed")]
    internal static partial void LogHandlerFailed(this ILogger logger, Exception exception, string brokerType, string eventType, Guid brokerId);

    // ---------------------------------------------
    // Keyed subscriptions: 1100-1199
    // ---------------------------------------------

    [LoggerMessage(EventId = 1100, Level = LogLevel.Debug, Message = "[{BrokerType}<{KeyType}, {EventType}> | {BrokerId}] Subscriber added for key {Key}. Count: {Count}")]
    internal static partial void LogKeyedSubscriberAdded(this ILogger logger, string brokerType, string keyType, string eventType, Guid brokerId, object? key, int count);

    [LoggerMessage(EventId = 1101, Level = LogLevel.Debug, Message = "[{BrokerType}<{KeyType}, {EventType}> | {BrokerId}] Subscriber removed for key {Key}. Count: {Count}")]
    internal static partial void LogKeyedSubscriberRemoved(this ILogger logger, string brokerType, string keyType, string eventType, Guid brokerId, object? key, int count);

    [LoggerMessage(EventId = 1102, Level = LogLevel.Debug, Message = "[{BrokerType}<{KeyType}, {EventType}> | {BrokerId}] Removed {Count} subscribers for key {Key}")]
    internal static partial void LogKeyedSubscribersRemoved(this ILogger logger, string brokerType, string keyType, string eventType, Guid brokerId, object? key, int count);

    [LoggerMessage(EventId = 1103, Level = LogLevel.Trace, Message = "[{BrokerType}<{KeyType}, {EventType}> | {BrokerId}] Created subscription list for key {Key}")]
    internal static partial void LogKeyedSubscriptionListCreated(this ILogger logger, string brokerType, string keyType, string eventType, Guid brokerId, object? key);

    [LoggerMessage(EventId = 1104, Level = LogLevel.Trace, Message = "[{BrokerType}<{KeyType}, {EventType}> | {BrokerId}] Removed empty subscription list for key {Key}")]
    internal static partial void LogKeyedSubscriptionListRemoved(this ILogger logger, string brokerType, string keyType, string eventType, Guid brokerId, object? key);

    [LoggerMessage(EventId = 1105, Level = LogLevel.Debug, Message = "[{BrokerType}<{KeyType}, {EventType}> | {BrokerId}] No subscriptions found for key {Key}")]
    internal static partial void LogKeyNotFound(this ILogger logger, string brokerType, string keyType, string eventType, Guid brokerId, object? key);

    [LoggerMessage(EventId = 1106, Level = LogLevel.Error, Message = "[{BrokerType}<{KeyType}, {EventType}> | {BrokerId}] Handler invocation failed for key {Key}")]
    internal static partial void LogKeyedHandlerFailed(this ILogger logger, Exception exception, string brokerType, string keyType, string eventType, Guid brokerId, object? key);
    
    // ---------------------------------------------
    // Message queues: 1200-1299
    // ---------------------------------------------

    [LoggerMessage(EventId = 1200, Level = LogLevel.Error, Message = "Message queue for {MessageType} stopped because its handler failed")]
    internal static partial void LogQueueHandlerFailed(this ILogger logger, Exception exception, string messageType);

    [LoggerMessage(EventId = 1201, Level = LogLevel.Information, Message = "Message queue for {MessageType} stopped due to cancellation")]
    internal static partial void LogQueueHandlerCanceled(this ILogger logger, Exception exception, string messageType);
}
