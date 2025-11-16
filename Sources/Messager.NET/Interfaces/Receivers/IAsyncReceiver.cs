namespace Messager.NET.Interfaces.Receivers;

/// <summary>
/// 
/// </summary>
/// <typeparam name="TEvent"></typeparam>
public interface IAsyncReceiver<out TEvent>
{
	/// <summary>
	/// 
	/// </summary>
	/// <param name="handler"></param>
	/// <returns></returns>
	public IAsyncDisposable Subscribe(Func<TEvent, ValueTask> handler);
	
	/// <summary>
	/// 
	/// </summary>
	/// <param name="handler"></param>
	public void Unsubscribe(Func<TEvent, ValueTask> handler);
}

/// <summary>
/// 
/// </summary>
/// <typeparam name="TKey"></typeparam>
/// <typeparam name="TEvent"></typeparam>
public interface IAsyncReceiver<in TKey, out TEvent> where TKey : notnull
{
	/// <summary>
	/// 
	/// </summary>
	/// <param name="key"></param>
	/// <param name="handler"></param>
	/// <returns></returns>
	public IAsyncDisposable Subscribe(TKey key, Func<TEvent, ValueTask> handler);
	
	/// <summary>
	/// 
	/// </summary>
	/// <param name="key"></param>
	/// <param name="handler"></param>
	public void Unsubscribe(TKey key, Func<TEvent, ValueTask> handler);
}