namespace Messager.NET.Interfaces.Requests;

public interface IRequest<out TOut>
{
	public TOut Invoke();
} 

public interface IRequest<out TOut, in TIn>
{
	public TOut Invoke(TIn input);
}

public interface IRequest<in TKey, out TOut, in TIn> 
	where TKey : notnull
{
	public TOut Invoke(TKey key, TIn input);
}

