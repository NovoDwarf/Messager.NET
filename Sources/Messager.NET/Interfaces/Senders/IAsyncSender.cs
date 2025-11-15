namespace Messager.NET.Interfaces.Senders;

/// <summary>
/// 
/// </summary>
/// <typeparam name="TEvent"></typeparam>
public interface IAsyncSender<in TEvent>
{ 
	/// <summary>
	/// 
	/// </summary>
	/// <param name="evt"></param>
	/// <returns></returns>
	public ValueTask SendAsync(TEvent evt);
}

/// <summary>
/// 
/// </summary>
/// <typeparam name="TKey"></typeparam>
/// <typeparam name="TEvent"></typeparam>
public interface IAsyncSender<in TKey, in TEvent> where TKey : notnull
{
	/// <summary>
	/// 
	/// </summary>
	/// <param name="key"></param>
	/// <param name="evt"></param>
	/// <returns></returns>
	public ValueTask SendAsync(TKey key, TEvent evt);
}