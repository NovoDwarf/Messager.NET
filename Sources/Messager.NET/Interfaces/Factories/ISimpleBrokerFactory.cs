using Messager.NET.Interfaces.Receivers;
using Messager.NET.Interfaces.Senders;

namespace Messager.NET.Interfaces.Factories;

public interface ISimpleBrokerFactory
{
	ISender<TEvent> GetSender<TEvent>();
	IReceiver<TEvent> GetReceiver<TEvent>();

	IAsyncSender<TEvent> GetAsyncSender<TEvent>();
	IAsyncReceiver<TEvent> GetAsyncReceiver<TEvent>();
}