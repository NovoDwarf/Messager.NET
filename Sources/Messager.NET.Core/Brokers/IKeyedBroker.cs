namespace Messager.NET.Brokers;

internal interface IKeyedBroker : ISimpleBroker
{
	public string KeyType { get; }
}