namespace Messager.NET.Interfaces.Requests;

/// <summary>
/// 
/// </summary>
/// <typeparam name="TOutput"></typeparam>
/// <typeparam name="TInput"></typeparam>
public interface IRequest<out TOutput, in TInput>
{
	/// <summary>
	/// 
	/// </summary>
	/// <param name="input"></param>
	/// <returns></returns>
	public TOutput Invoke(TInput input);
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

