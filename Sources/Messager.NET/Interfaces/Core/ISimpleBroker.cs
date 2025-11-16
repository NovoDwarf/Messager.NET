namespace Messager.NET.Interfaces.Core;

internal interface ISimpleBroker
{

	public Guid Id { get; internal set; }

	public string BrokerType { get; }
	public string EventType { get; }
}