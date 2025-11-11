namespace Messager.NET.Interfaces.Core;

public interface IBroker
{
	public Guid Id { get; internal set; }
	
	public string BrokerType { get; }
	public string EventType { get; }
}