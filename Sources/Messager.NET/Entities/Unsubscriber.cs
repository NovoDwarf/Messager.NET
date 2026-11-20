namespace Messager.NET.Entities;

/// <summary>
/// Provides a thread-safe mechanism for unsubscribing from events or notifications
/// by invoking a specified action when disposed.
/// </summary>
/// <remarks>
/// This class ensures that the unsubscribe action is invoked exactly once,
/// even when <see cref="Dispose"/> is called concurrently from multiple threads.
/// After disposal, subsequent calls to <see cref="Dispose"/> have no effect.
/// </remarks>
public sealed class Unsubscriber : IDisposable
{
    private Action? _unsubscribe;

    public Unsubscriber(Action unsubscribe)
    {
        ArgumentNullException.ThrowIfNull(unsubscribe);
        
        _unsubscribe = unsubscribe;
    }

    /// <summary>
    /// Gets whether unsubscription has been initiated.
    /// </summary>
    public bool IsDisposed => Volatile.Read(ref _unsubscribe) is null;

    /// <summary>
    /// Executes the unsubscribe action at most once.
    /// Thread-safe and idempotent.
    /// </summary>
    public void Dispose()
    {
        Interlocked.Exchange(ref _unsubscribe, null)?.Invoke();
    }
}
