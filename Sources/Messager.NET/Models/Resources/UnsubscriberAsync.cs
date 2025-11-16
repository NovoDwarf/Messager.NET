namespace Messager.NET.Models.Resources;

/// <summary>
/// Provides a mechanism for asynchronous unsubscription that ensures the unsubscribe operation
/// is performed only once, even if <see cref="DisposeAsync"/> is called multiple times.
/// </summary>
/// <remarks>
/// This class is thread-safe and implements the asynchronous disposal pattern.
/// The unsubscribe delegate is invoked exactly once when <see cref="DisposeAsync"/> is first called.
/// Subsequent calls to <see cref="DisposeAsync"/> are ignored.
/// </remarks>
public sealed class UnsubscriberAsync : IAsyncDisposable
{
	private readonly Func<ValueTask> _unsubscribe;
	
	/// <summary>
	/// Initializes a new instance of the <see cref="UnsubscriberAsync"/> class
	/// with the specified unsubscribe function.
	/// </summary>
	/// <param name="unsubscribe">
	/// The asynchronous function to execute when disposing. 
	/// This function should contain the unsubscription logic.
	/// Must not be null.
	/// </param>
	/// <exception cref="ArgumentNullException">
	/// Thrown when <paramref name="unsubscribe"/> is null.
	/// </exception>
	public UnsubscriberAsync(Func<ValueTask> unsubscribe)
	{
		ArgumentNullException.ThrowIfNull(unsubscribe);
		
		_unsubscribe = unsubscribe;
	}
	
	/// <summary>
	/// Gets a value indicating whether the unsubscription has been performed.
	/// </summary>
	public bool IsDisposed => _disposed != 0;
	
	private int _disposed;
	
	/// <summary>
	/// Asynchronously executes the unsubscribe operation if it hasn't been performed already.
	/// </summary>
	/// <remarks>
	/// This method is thread-safe and idempotent. The first call to this method
	/// will execute the unsubscribe function, and any subsequent calls will be ignored.
	/// The method uses interlocked operations to ensure thread safety.
	/// </remarks>
	/// <returns>
	/// A <see cref="ValueTask"/> that represents the asynchronous unsubscription operation.
	/// </returns>
	public async ValueTask DisposeAsync()
	{
		if (Interlocked.CompareExchange(ref _disposed, 1, 0) != 0)
			return;

		await _unsubscribe().ConfigureAwait(false);
	}
}