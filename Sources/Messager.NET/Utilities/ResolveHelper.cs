using JetBrains.Annotations;
using Messager.NET.Requests;

namespace Messager.NET.Utilities;

/// <summary>Identifies request handler types and interfaces.</summary>
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

	/// <summary>Determines whether the type implements a request interface.</summary>
	[Pure]
	public static bool IsRequestType(Type type)
	{
		return type.GetInterfaces().Any(IsRequestInterface);
	}

	/// <summary>Determines whether the type is a supported request interface.</summary>
	[Pure]
	public static bool IsRequestInterface(Type type)
	{
		return type.IsGenericType && RequestTypes.Contains(type.GetGenericTypeDefinition());
	}
}