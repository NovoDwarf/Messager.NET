namespace Messager.NET.Brokers;

/// <summary>
/// Internal metadata shared by keyed broker implementations.
/// </summary>
internal interface IKeyedBroker : ISimpleBroker
{
	/// <summary>
	/// Key type name handled by the broker.
	/// </summary>
	public string KeyType { get; }
}
