namespace Messager.NET.Interfaces.Core;

public interface IKeyedBroker : IBroker
{
	public string KeyType { get; }
}