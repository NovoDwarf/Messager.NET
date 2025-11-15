namespace Messager.NET.Interfaces.Core;

internal interface IKeyedBroker : IBroker
{
	public string KeyType { get; }
}