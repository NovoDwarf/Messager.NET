namespace Messager.NET.Brokers;

/// <summary>
/// Marks a request broker with a stable runtime identifier for diagnostics.
/// </summary>
public interface IRequestBroker
{
	/// <summary>
	/// Unique identifier of the broker instance.
	/// </summary>
	public Guid Id { get; internal set; }
}
