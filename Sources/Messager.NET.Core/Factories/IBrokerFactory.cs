using Messager.NET.Receivers;
using Messager.NET.Senders;

namespace Messager.NET.Factories;

/// <summary>
/// 
/// </summary>
public interface IBrokerFactory
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