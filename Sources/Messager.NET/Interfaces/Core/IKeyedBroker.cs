namespace Messager.NET.Interfaces.Core;

/// <summary>
/// 
/// </summary>
public interface IKeyedBroker : IBroker
{
	/// <summary>
	/// 
	/// </summary>
	public string KeyType { get; }
}