namespace Messager.NET.Interfaces.Core;

internal interface IKeyedBroker : ISimpleBroker
{
	public string KeyType { get; }
}