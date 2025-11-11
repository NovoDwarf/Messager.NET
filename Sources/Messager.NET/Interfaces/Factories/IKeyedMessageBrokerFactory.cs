using Messager.NET.Interfaces.Receivers;
using Messager.NET.Interfaces.Senders;

namespace Messager.NET.Interfaces.Factories;

public interface IKeyedMessageBrokerFactory
{
	ISender<TKey, TEvent> GetKeyedSender<TKey, TEvent>() where TKey : notnull;
	IReceiver<TKey, TEvent> GetKeyedReceiver<TKey, TEvent>() where TKey : notnull;

	IAsyncSender<TKey, TEvent> GetAsyncKeyedSender<TKey, TEvent>() where TKey : notnull;
	IAsyncReceiver<TKey, TEvent> GetAsyncKeyedReceiver<TKey, TEvent>() where TKey : notnull;
}