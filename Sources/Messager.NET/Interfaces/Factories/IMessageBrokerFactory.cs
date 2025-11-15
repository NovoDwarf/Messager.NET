using Messager.NET.Interfaces.Receivers;
using Messager.NET.Interfaces.Senders;

namespace Messager.NET.Interfaces.Factories;

/// <summary>
/// 
/// </summary>
public interface IMessageBrokerFactory
{
	/// <summary>
	/// B
	/// </summary>
	/// <typeparam name="TEvent"></typeparam>
	/// <returns></returns>
	ISender<TEvent> GetSender<TEvent>();
	
	/// <summary>
	/// 
	/// </summary>
	/// <typeparam name="TEvent"></typeparam>
	/// <returns></returns>
	IReceiver<TEvent> GetReceiver<TEvent>();

	/// <summary>
	/// 
	/// </summary>
	/// <typeparam name="TEvent"></typeparam>
	/// <returns></returns>
	IAsyncSender<TEvent> GetAsyncSender<TEvent>();
	
	/// <summary>
	/// 
	/// </summary>
	/// <typeparam name="TEvent"></typeparam>
	/// <returns></returns>
	IAsyncReceiver<TEvent> GetAsyncReceiver<TEvent>();
}