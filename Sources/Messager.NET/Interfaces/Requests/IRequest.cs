namespace Messager.NET.Interfaces.Requests;

public interface IRequest<out TOut>
{
	/// <summary>
	/// 
	/// </summary>
	/// <param name="input"></param>
	/// <returns></returns>
	public TOut Invoke();
} 

/// <summary>
/// 
/// </summary>
/// <typeparam name="TOutput"></typeparam>
/// <typeparam name="TInput"></typeparam>
public interface IRequest<out TOut, in TIn>
{
	/// <summary>
	/// 
	/// </summary>
	/// <param name="input"></param>
	/// <returns></returns>
	public TOut Invoke(TIn input);
}

/// <summary>
/// 
/// </summary>
/// <typeparam name="TKey"></typeparam>
/// <typeparam name="TOut"></typeparam>
/// <typeparam name="TIn"></typeparam>
public interface IRequest<in TKey, out TOut, in TIn> 
	where TKey : notnull
{
	/// <summary>
	/// 
	/// </summary>
	/// <param name="key"></param>
	/// <param name="input"></param>
	/// <returns></returns>
	public TOut Invoke(TKey key, TIn input);
}

