using Messager.NET.Brokers;
using Messager.NET.Entities;
using Messager.NET.Extensions;
using Messager.NET.Models.Resources;
using Messager.NET.Receivers;
using Messager.NET.Senders;
using Microsoft.Extensions.Logging;

namespace Messager.NET.Models.Brokers;

public class SimpleBroker<TEvent> : ISimpleBroker, ISender<TEvent>, IReceiver<TEvent>
{
	private readonly List<WeakAction<TEvent>> _handlers = [];
	private readonly object _locker = new();
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
		List<WeakAction<TEvent>> activeHandlers;
		
		lock (_locker)
		{
			TryRemove();
			activeHandlers = _handlers.ToList();
		}

		foreach (var sub in activeHandlers) 
			TryInvoke(sub, evt);
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

		_logger?.LogSubscribersRemoved(BrokerType, EventType, Id, removedCount);
	}

	private void TryInvoke(WeakAction<TEvent> sub, TEvent evt)
	{
		try
		{
			sub.TryInvoke(evt);
		}
		catch (Exception ex)
		{
			_logger?.LogErrorInvokingHandler(ex, BrokerType, EventType, Id);
		}
	}
}
