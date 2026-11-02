namespace Messager.NET.Queues;

/// <summary>
/// Accepts messages for background processing within the current process.
/// </summary>
/// <typeparam name="TMessage">The message payload type.</typeparam>
public interface IMessageQueue<in TMessage>
{
    /// <summary>
    /// Waits for capacity and admits a message to the queue.
    /// Completion confirms admission, not successful processing.
    /// </summary>
    /// <remarks>
    /// Cancellation only affects admission and does not retract an accepted message.
    /// A closed queue rejects writes with a ChannelClosedException, or an OperationCanceledException
    /// when the write or the queue's handler was canceled.
    /// </remarks>
    public ValueTask EnqueueAsync(TMessage message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Attempts immediate admission without waiting for capacity.
    /// Returns false when the queue is full or closed.
    /// </summary>
    public bool TryEnqueue(TMessage message);
}
