namespace Messager.NET.Models.Resources;

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
    /// <summary>
    /// The unsubscribe action to execute. Volatile for proper thread visibility.
    /// Set to null after invocation to prevent duplicate execution.
    /// </summary>
    private Action? _unsubscribe;

    /// <summary>
    /// Initializes a new instance of the <see cref="Unsubscriber"/> class
    /// with the specified unsubscribe action.
    /// </summary>
    /// <param name="unsubscribe">
    /// The action to execute when disposing. This typically contains logic
    /// to remove event handlers or cancel subscriptions.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="unsubscribe"/> is null.
    /// </exception>
    public Unsubscriber(Action unsubscribe)
    {
        ArgumentNullException.ThrowIfNull(unsubscribe);
        
        _unsubscribe = unsubscribe;
    }

    /// <summary>
    /// Gets a value indicating whether the unsubscriber has been disposed.
    /// </summary>
    /// <value>
    /// <c>true</c> if the unsubscriber has been disposed; otherwise, <c>false</c>.
    /// </value>
    public bool IsDisposed => _unsubscribe == null;

    /// <summary>
    /// Executes the unsubscribe action if it hasn't been executed already.
    /// This method is thread-safe and idempotent.
    /// </summary>
    /// <remarks>
    /// The unsubscribe action is invoked exactly once, regardless of how many times
    /// this method is called or from how many threads. Subsequent calls are ignored.
    /// </remarks>
    public void Dispose()
    {
        var action = Interlocked.Exchange(ref _unsubscribe, null);
        
        action?.Invoke();
    }
}