using Messager.NET.Interfaces.Requests;

namespace Messager.NET.Entity.Requests;

public class Request<TOut, TIn> : IRequest<TOut, TIn>
{
	private readonly IRequest<TOut, TIn> _impl;

	public Request(IRequest<TOut, TIn> impl)
	{
		_impl = impl;
	}

	public TOut Invoke(TIn input) => _impl.Invoke(input);
}

public class Request<TKey, TOut, TIn> : IRequest<TKey, TOut, TIn> 
	where TKey : notnull
{
	private readonly IRequest<TKey, TOut, TIn> _impl;
    
	public Request(IRequest<TKey, TOut, TIn> impl)
	{
		_impl = impl;
	}
	
	public TOut Invoke(TKey key, TIn input) => _impl.Invoke(key, input);
}