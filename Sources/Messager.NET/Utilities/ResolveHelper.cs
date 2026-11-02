using Messager.NET.Requests;

namespace Messager.NET.Utilities;

public static class ResolveHelper
{
	private static readonly HashSet<Type> RequestTypes =
	[
		typeof(IRequest<>),
		typeof(IRequest<,>),
		typeof(IRequest<,,>),
		typeof(IAsyncRequest<>),
		typeof(IAsyncRequest<,>),
		typeof(IAsyncRequest<,,>)
	];

	public static bool IsRequestType(Type type)
	{
		return type.GetInterfaces().Any(IsRequestInterface);
	}

	public static bool IsRequestInterface(Type type)
	{
		return type.IsGenericType && RequestTypes.Contains(type.GetGenericTypeDefinition());
	}
}
