namespace Messager.NET.Interfaces.Core;

/// <summary>
/// 
/// </summary>
public interface IBroker
{
	/// <summary>
	/// 
	/// </summary>
	public Guid Id { get; internal set; }
	
	/// <summary>
	/// 
	/// </summary>
	public string BrokerType { get; }
	
	/// <summary>
	/// 
	/// </summary>
	public string EventType { get; }
}