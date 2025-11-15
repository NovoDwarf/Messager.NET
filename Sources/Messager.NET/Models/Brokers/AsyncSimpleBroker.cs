using Messager.NET.Extensions;
using Messager.NET.Interfaces.Core;
using Messager.NET.Interfaces.Receivers;
using Messager.NET.Interfaces.Senders;
using Messager.NET.Models.Resources;
using Microsoft.Extensions.Logging;

namespace Messager.NET.Models.Brokers;

public class AsyncSimpleBroker<TEvent> : IBroker, IAsyncSender<TEvent>, IAsyncReceiver<TEvent>
{
	private readonly List<Func<TEvent, ValueTask>> _handlers = [];
	private readonly Lock _locker = new();
	private readonly ILogger<AsyncSimpleBroker<TEvent>>? _logger;

	public AsyncSimpleBroker(ILogger<AsyncSimpleBroker<TEvent>>? logger = null)
	{
		_logger = logger;
	}
	
	public Guid Id { get; set; } = Guid.NewGuid();
	
	public string BrokerType => typeof(SimpleBroker<>).Name;
	public string EventType => typeof(TEvent).Name;

	public async ValueTask SendAsync(TEvent evt)
	{
		List<Func<TEvent, ValueTask>> copy;
		
		lock (_locker)
		{
			copy = _handlers.ToList();
		}

		foreach (var handler in copy) 
			await TryInvokeAsync(handler, evt);
	}
	
	public IAsyncDisposable Subscribe(Func<TEvent, ValueTask> handler)
	{
		lock (_locker)
		{
			_handlers.Add(handler);
			_logger?.LogSubscriberAdded(BrokerType, EventType, Id);
		}

		return new AsyncUnsubscriber(() =>
		{
			lock (_locker)
			{
				_handlers.Remove(handler);
				_logger?.LogSubscriberRemoved(BrokerType, EventType, Id);
				
				return ValueTask.CompletedTask;
			}
		});
	}

	public void Unsubscribe(Func<TEvent, ValueTask> handler)
	{
		lock (_locker)
		{
			if (_handlers.Remove(handler)) 
				_logger?.LogSubscriberRemoved(BrokerType, EventType, Id);
		}
	}
	
	private async Task TryInvokeAsync(Func<TEvent, ValueTask> handler, TEvent evt)
	{
		try
		{
			await handler(evt);
		}
		catch (Exception ex)
		{
			_logger?.LogErrorInvokingHandler(ex, BrokerType, EventType, Id);
		}
	}
}