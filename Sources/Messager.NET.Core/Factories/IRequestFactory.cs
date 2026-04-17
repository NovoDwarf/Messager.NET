using Messager.NET.Requests;

namespace Messager.NET.Factories;

public interface IRequestFactory
{
	public IRequest<TOut> Resolve<TOut>();
	public IEnumerable<IRequest<TOut>> ResolveAll<TOut>();
	
	public IRequest<TOut, TIn> Resolve<TOut, TIn>();
	public IEnumerable<IRequest<TOut, TIn>> ResolveAll<TOut, TIn>();
	
	public IRequest<TKey, TOut, TIn> Resolve<TKey, TOut, TIn>() where TKey : notnull;
	public IEnumerable<IRequest<TKey, TOut, TIn>> ResolveAll<TKey, TOut, TIn>() where TKey : notnull;
}

public interface IAsyncRequestFactory
{
	public IAsyncRequest<TOut> Resolve<TOut>();
	public IEnumerable<IAsyncRequest<TOut>> ResolveAll<TOut>();
	
	public IAsyncRequest<TOut, TIn> Resolve<TOut, TIn>();
	public IEnumerable<IAsyncRequest<TOut, TIn>> ResolveAll<TOut, TIn>();
	
	public IAsyncRequest<TKey, TOut, TIn> Resolve<TKey, TOut, TIn>() where TKey : notnull;
	public IEnumerable<IAsyncRequest<TKey, TOut, TIn>> ResolveAll<TKey, TOut, TIn>() where TKey : notnull;
}