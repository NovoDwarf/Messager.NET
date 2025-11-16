using Messager.NET.Interfaces.Requests;

namespace Messager.NET.Interfaces.Factories;

public interface IRequestFactory
{
	public IRequest<TOut> Resolve<TOut>();
	public IEnumerable<IRequest<TOut>> ResolveAll<TOut>();
}