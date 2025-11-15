using Messager.NET.Interfaces.Core;
using Messager.NET.Interfaces.Receivers;
using Messager.NET.Interfaces.Senders;
using Messager.NET.Models.Resources;
using Microsoft.Extensions.Logging;

namespace Messager.NET.Models.Brokers;

public class AsyncKeyedBroker<TKey, TEvent> : IKeyedBroker, IAsyncSender<TKey, TEvent>, IAsyncReceiver<TKey, TEvent>
	where TKey : notnull
{
	private readonly Dictionary<TKey, List<Func<TEvent, ValueTask>>> _handlers = new();
	private readonly Lock _locker = new();
	private readonly ILogger<AsyncKeyedBroker<TKey, TEvent>>? _logger;

	public AsyncKeyedBroker(ILogger<AsyncKeyedBroker<TKey, TEvent>>? logger = null)
	{
		_logger = logger;
	}
	
	public Guid Id { get; set; } = Guid.NewGuid();
	
	public string BrokerType => typeof(AsyncKeyedBroker<TKey, TEvent>).Name;
	public string KeyType => typeof(TKey).Name;
	public string EventType => typeof(TEvent).Name;
	
	public IAsyncDisposable Subscribe(TKey key, Func<TEvent, ValueTask> handler)
	{
		lock (_locker)
		{
			if (!_handlers.TryGetValue(key, out var list))
			{
				list = [];
				_handlers[key] = list;
				
			}
			list.Add(handler);
			_logger?.LogDebug("Subscriber added for key {Key} in AsyncKeyedBroker<{KeyType}, {EventType}>. Total subscribers for key: {Count}", key, typeof(TKey).Name, typeof(TEvent).Name, list.Count);
		}

		return new UnsubscriberAsync(() =>
		{
			lock (_locker)
			{
				if (!_handlers.TryGetValue(key, out var list))
				{ 
					
					return ValueTask.CompletedTask;
				}
				
				list.Remove(handler);
	
				if (list.Count == 0)
				{
					_handlers.Remove(key);
				}
				
				return ValueTask.CompletedTask;
			}
		});
	}

	public void Unsubscribe(TKey key, Func<TEvent, ValueTask> handler)
	{
		lock (_locker)
		{
			if (!_handlers.TryGetValue(key, out var list))
			{
				return;
			}
			
			var removed = list.Remove(handler);
			
			if (removed)
			{ }
			
			if (list.Count == 0)
			{
				_handlers.Remove(key);
			}
		}
	}

	public async ValueTask SendAsync(TKey key, TEvent evt)
	{
		List<Func<TEvent, ValueTask>>? handlersCopy = null;

		lock (_locker)
		{
			if (_handlers.TryGetValue(key, out var list))
			{
				handlersCopy = list.ToList();
			}
		}

		if (handlersCopy == null || handlersCopy.Count == 0)
			return;

		foreach (var handler in handlersCopy)
			await TryInvokeAsync(handler, key, evt);
	}
	
	private async Task TryInvokeAsync(Func<TEvent, ValueTask> handler, TKey key, TEvent evt)
	{
		try
		{
			await handler(evt);
		}
		catch (Exception ex)
		{
			_logger?.LogError(ex, "Error invoking async handler for event {EventType} with key {Key}", typeof(TEvent).Name, key);
		}
	}
}