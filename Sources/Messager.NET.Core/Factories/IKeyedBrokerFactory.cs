using Messager.NET.Receivers;
using Messager.NET.Senders;

namespace Messager.NET.Factories;

/// <summary>
/// 
/// </summary>
public interface IKeyedBrokerFactory
{
	/// <summary>
	/// 
	/// </summary>
	/// <typeparam name="TKey"></typeparam>
	/// <typeparam name="TEvent"></typeparam>
	/// <returns></returns>
	ISender<TKey, TEvent> GetKeyedSender<TKey, TEvent>() where TKey : notnull;
	
	/// <summary>
	/// 
	/// </summary>
	/// <typeparam name="TKey"></typeparam>
	/// <typeparam name="TEvent"></typeparam>
	/// <returns></returns>
	IReceiver<TKey, TEvent> GetKeyedReceiver<TKey, TEvent>() where TKey : notnull;

	/// <summary>
	/// 
	/// </summary>
	/// <typeparam name="TKey"></typeparam>
	/// <typeparam name="TEvent"></typeparam>
	/// <returns></returns>
	IAsyncSender<TKey, TEvent> GetAsyncKeyedSender<TKey, TEvent>() where TKey : notnull;
	
	/// <summary>
	/// 
	/// </summary>
	/// <typeparam name="TKey"></typeparam>
	/// <typeparam name="TEvent"></typeparam>
	/// <returns></returns>
	IAsyncReceiver<TKey, TEvent> GetAsyncKeyedReceiver<TKey, TEvent>() where TKey : notnull;
}