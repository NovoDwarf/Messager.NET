namespace Messager.NET.Interfaces.Receivers;

/// <summary>
/// 
/// </summary>
/// <typeparam name="TEvent"></typeparam>
public interface IReceiver<out TEvent>
{
	/// <summary>
	/// 
	/// </summary>
	/// <param name="handler"></param>
	/// <returns></returns>
	public IDisposable Subscribe(Action<TEvent> handler);
}

/// <summary>
/// 
/// </summary>
/// <typeparam name="TKey"></typeparam>
/// <typeparam name="TEvent"></typeparam>
public interface IReceiver<in TKey, out TEvent>
{
	/// <summary>
	/// 
	/// </summary>
	/// <param name="key"></param>
	/// <param name="handler"></param>
	/// <returns></returns>
	public IDisposable Subscribe(TKey key, Action<TEvent> handler);
}