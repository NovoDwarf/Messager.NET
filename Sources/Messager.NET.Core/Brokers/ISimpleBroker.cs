namespace Messager.NET.Brokers;

/// <summary>
/// Internal metadata shared by broker implementations.
/// </summary>
internal interface ISimpleBroker
{
	/// <summary>
	/// Unique identifier of the broker instance.
	/// </summary>
	public Guid Id { get; internal set; }

	/// <summary>
	/// Runtime broker type name used in diagnostics.
	/// </summary>
	public string BrokerType { get; }

	/// <summary>
	/// Event type name handled by the broker.
	/// </summary>
	public string EventType { get; }
}
