namespace Messager.NET.Interfaces.Senders;

/// <summary>
/// 
/// </summary>
/// <typeparam name="TEvent"></typeparam>
public interface ISender<in TEvent>
{
	/// <summary>
	/// 
	/// </summary>
	/// <param name="evt"></param>
	public void Send(TEvent evt);
}

/// <summary>
/// 
/// </summary>
/// <typeparam name="TKey"></typeparam>
/// <typeparam name="TEvent"></typeparam>
public interface ISender<in TKey, in TEvent> where TKey : notnull
{
	/// <summary>
	/// 
	/// </summary>
	/// <param name="key"></param>
	/// <param name="evt"></param>
	public void Send(TKey key, TEvent evt);
}