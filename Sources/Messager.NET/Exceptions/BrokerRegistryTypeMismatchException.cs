namespace Messager.NET.Exceptions;

internal sealed class BrokerRegistryTypeMismatchException : InvalidOperationException
{
	public BrokerRegistryTypeMismatchException(object? key, Type expectedType, Type? actualType) : base($"Broker registered for key '{key}' has type '{actualType?.FullName ?? "null"}', but '{expectedType.FullName}' was expected.")
	{
		Key = key;
		ExpectedType = expectedType;
		ActualType = actualType;
	}

	public object? Key { get; }
	public Type ExpectedType { get; }
	public Type? ActualType { get; }
}