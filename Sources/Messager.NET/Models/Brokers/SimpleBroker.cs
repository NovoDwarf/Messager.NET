using Messager.NET.Core;
using Messager.NET.Extensions;
using Messager.NET.Interfaces.Core;
using Messager.NET.Interfaces.Receivers;
using Messager.NET.Interfaces.Senders;
using Messager.NET.Models.Resources;
using Microsoft.Extensions.Logging;

namespace Messager.NET.Models.Brokers;

public class SimpleBroker<TEvent> : IBroker, ISender<TEvent>, IReceiver<TEvent>
{
	private readonly List<WeakAction<TEvent>> _handlers = [];
	private readonly Lock _locker = new();
	private readonly ILogger<SimpleBroker<TEvent>>? _logger;

	public SimpleBroker(ILogger<SimpleBroker<TEvent>>? logger = null)
	{
		_logger = logger;
	}

	public Guid Id { get; set; } = Guid.NewGuid();
	
	public string BrokerType => typeof(SimpleBroker<>).Name;
	public string EventType => typeof(TEvent).Name;
	
	public void Send(TEvent evt)
	{
		TryRemove();
		
		lock (_locker)
		{
			var activeHandlers = _handlers.ToList();
			
			foreach (var sub in activeHandlers) 
				TryInvoke(sub, evt);
		}
	}

	public IDisposable Subscribe(Action<TEvent> handler)
	{
		lock (_locker)
		{
			_handlers.Add(new WeakAction<TEvent>(handler));
			_logger?.LogSubscriberAdded(BrokerType, EventType, Id);
		}

		return new Unsubscriber(() =>
		{
			lock (_locker)
			{
				_handlers.RemoveAll(s => s.Matches(handler));
				_logger?.LogSubscriberRemoved(BrokerType, EventType, Id);
			}
		});
	}

	private void TryRemove()
	{
		var removedCount = _handlers.RemoveAll(s => !s.IsAlive);

		if (removedCount <= 0) 
			return;
		
		lock (_locker)
		{
			_logger?.LogSubscribersRemoved(BrokerType, EventType, Id, removedCount);
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