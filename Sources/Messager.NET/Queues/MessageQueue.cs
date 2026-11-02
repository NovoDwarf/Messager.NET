using System.Threading.Channels;
using Messager.NET.Extensions;
using Microsoft.Extensions.Logging;

namespace Messager.NET.Queues;

/// <summary>
/// Processes messages sequentially on a background worker using a bounded in-memory channel.
/// </summary>
/// <remarks>
/// The owner must complete or asynchronously dispose the queue and observe its completion.
/// The first handler failure stops processing and discards the remaining buffered messages.
/// Messages and callbacks are retained by strong references while needed by the queue.
/// </remarks>
public sealed class MessageQueue<TMessage> : IMessageQueue<TMessage>, IAsyncDisposable
{
    private readonly Channel<TMessage> _channel;
    private readonly ILogger<MessageQueue<TMessage>>? _logger;

    /// <summary>
    /// Creates a queue and starts its worker. Multicast handlers are awaited in invocation-list order.
    /// </summary>
    /// <param name="handler">The strongly retained message handler.</param>
    /// <param name="capacity">The maximum number of buffered messages, excluding the active message.</param>
    /// <param name="logger">An optional logger. Failures remain observable without logging.</param>
    public MessageQueue(
        Func<TMessage, ValueTask> handler,
        int capacity = 1024,
        ILogger<MessageQueue<TMessage>>? logger = null)
    {
        ArgumentNullException.ThrowIfNull(handler);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(capacity);

        _callbacks = handler.GetInvocationList();
        _logger = logger;
        _channel = Channel.CreateBounded<TMessage>(new BoundedChannelOptions(capacity)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
            SingleWriter = false,
            AllowSynchronousContinuations = false
        });
        
        Completion = Task.Run(ProcessAsync);
    }

    /// <summary>
    /// Completes after processing stops. Faults on handler failure and is canceled on handler cancellation.
    /// </summary>
    public Task Completion { get; }

    private Delegate[]? _callbacks;
    
    /// <inheritdoc />
    public ValueTask EnqueueAsync(TMessage message, CancellationToken cancellationToken = default)
    {
        return _channel.Writer.WriteAsync(message, cancellationToken);
    }

    /// <inheritdoc />
    public bool TryEnqueue(TMessage message)
    {
        return _channel.Writer.TryWrite(message);
    }

    /// <summary>
    /// Stops admitting messages. The worker continues processing already accepted messages.
    /// Repeated calls have no additional effect.
    /// </summary>
    public void Complete()
    {
        _channel.Writer.TryComplete();
    }

    /// <summary>
    /// Stops admission and waits for the worker, propagating its outcome.
    /// </summary>
    /// <remarks>
    /// Cancellation stops only this wait; the worker continues draining the queue.
    /// Do not await this operation from the queue's own handler.
    /// </remarks>
    public Task CompleteAsync(CancellationToken cancellationToken = default)
    {
        Complete();
        
        return Completion.WaitAsync(cancellationToken);
    }

    /// <summary>
    /// Stops admission and waits for all accepted messages to be processed.
    /// Concurrent and repeated calls observe the same worker outcome, including failures.
    /// </summary>
    public ValueTask DisposeAsync()
    {
        return new ValueTask(CompleteAsync());
    }

    private async Task ProcessAsync()
    {
        try
        {
            await foreach (var message in _channel.Reader.ReadAllAsync().ConfigureAwait(false))
            {
                await InvokeAsync(message).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException exception)
        {
            _channel.Writer.TryComplete(exception);
            _logger?.LogQueueHandlerCanceled(exception, typeof(TMessage).FullName ?? typeof(TMessage).Name);
            throw;
        }
        catch (Exception exception)
        {
            _channel.Writer.TryComplete(exception);
            _logger?.LogQueueHandlerFailed(exception, typeof(TMessage).FullName ?? typeof(TMessage).Name);
            throw;
        }
        finally
        {
            _channel.Writer.TryComplete();

            // Failed queues must not retain payloads that will never be processed.
            while (_channel.Reader.TryRead(out _))
            {
            }

            _callbacks = null;
        }
    }

    private async ValueTask InvokeAsync(TMessage message)
    {
        foreach (var callback in _callbacks!)
        {
            await ((Func<TMessage, ValueTask>)callback)(message).ConfigureAwait(false);
        }
    }
}
