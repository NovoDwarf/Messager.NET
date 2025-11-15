using Messager.NET.Core;
using Messager.NET.Extensions;
using Messager.NET.Interfaces.Core;
using Messager.NET.Interfaces.Receivers;
using Messager.NET.Interfaces.Senders;
using Messager.NET.Models.Resources;
using Microsoft.Extensions.Logging;

namespace Messager.NET.Models.Brokers;

public partial class KeyedBroker<TKey, TEvent> : IKeyedBroker, ISender<TKey, TEvent>, IReceiver<TKey, TEvent> 
	where TKey : notnull
{
	private readonly Dictionary<TKey, List<WeakAction<TEvent>>> _handlers = new();
	private readonly Lock _locker = new();
	private readonly ILogger<KeyedBroker<TKey, TEvent>>? _logger;

	public KeyedBroker(ILogger<KeyedBroker<TKey, TEvent>>? logger = null)
	{
		_logger = logger;
	}
	
	public Guid Id { get; set; } = Guid.NewGuid();
	
	public string BrokerType => typeof(SimpleBroker<>).Name;
	public string KeyType => typeof(TKey).Name;
	public string EventType => typeof(TEvent).Name;
	
	public void Send(TKey key, TEvent evt)
	{
		lock (_locker)
		{
			if (!_handlers.TryGetValue(key, out var list))
			{
				_logger?.LogKeyNotFound(BrokerType, KeyType, EventType, Id, key.ToString() ?? "Unknown");
				return;
			}
			
			TryRemove(list);
			
			var activeHandlers = list.ToList();
			
			foreach (var sub in activeHandlers) 
				TryInvoke(sub, evt);

			if (list.Count != 0) 
				return;
			
			_handlers.Remove(key);
			_logger?.LogRemovedEmptyKey(BrokerType, KeyType, EventType, Id, key.ToString() ?? "Unknown");
		}
	}

	public IDisposable Subscribe(TKey key, Action<TEvent> handler)
	{
		lock (_locker)
		{
			if (!_handlers.TryGetValue(key, out var list))
			{
				list = [];
				_handlers[key] = list; 
				_logger?.LogCreateSubscriptionListForKey(BrokerType, KeyType, EventType, Id, key.ToString() ?? "Unknown");
			}
			list.Add(new WeakAction<TEvent>(handler));
			_logger?.LogSubscriberAddedForKey(BrokerType, KeyType, EventType, Id, key.ToString() ?? "Unknown");
		}

		return new Unsubscriber(() =>
		{
			lock (_locker)
			{
				if (!_handlers.TryGetValue(key, out var list)) 
					return;
				
				list.Remove(new WeakAction<TEvent>(handler));
				_logger?.LogSubscriberRemovedForKey(BrokerType, KeyType, EventType, Id, string.Empty);
				
				if (list.Count != 0) 
					return;
				
				_handlers.Remove(key);
				_logger?.LogRemovedEmptyKey(BrokerType, KeyType, EventType, Id, key.ToString() ?? "Unknown");
			}
		});
	}

	private void TryRemove(List<WeakAction<TEvent>> list)
	{
		var removedCount = list.RemoveAll(s => !s.IsAlive);

		if (removedCount <= 0) 
			return;
		
		lock (_locker)
		{
			_logger?.LogSubscribersRemovedForKey(BrokerType, KeyType, EventType, Id, string.Empty, removedCount);
		}
	}

	private void TryInvoke(WeakAction<TEvent> sub, TEvent evt)
	{
		try
		{
			sub.TryInvoke(evt);
		}
		catch (Exception ex)
		{
			lock (_locker)
			{
				_logger?.LogErrorInvokingHandler(ex, BrokerType, EventType, Id);
			}
		}
	}
}