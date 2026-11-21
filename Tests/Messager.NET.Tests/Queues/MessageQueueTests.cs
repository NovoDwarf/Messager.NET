using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading.Channels;
using Messager.NET.Queues;

namespace Messager.NET.Tests.Queues;

[TestFixture]
public sealed class MessageQueueTests
{
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(10);

    [TestCase(0)]
    [TestCase(-1)]
    public void Constructor_InvalidCapacity_Throws(int capacity)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new MessageQueue<int>(_ => ValueTask.CompletedTask, capacity));
    }

    [Test]
    public void Constructor_NullHandler_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new MessageQueue<int>(null!));
    }

    [Test]
    public async Task CompleteAsync_DrainsMessagesSequentiallyInAdmissionOrder()
    {
        var received = new List<int>();
        var active = 0;
        var maxActive = 0;
        await using var queue = new MessageQueue<int>(async message =>
        {
            active++;
            maxActive = Math.Max(maxActive, active);
            await Task.Yield();
            received.Add(message);
            active--;
        }, capacity: 3);

        for (var i = 0; i < 100; i++)
        {
            await queue.EnqueueAsync(i);
        }

        await queue.CompleteAsync().WaitAsync(Timeout);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(received, Is.EqualTo(Enumerable.Range(0, 100)));
            Assert.That(maxActive, Is.EqualTo(1));
            Assert.That(queue.Completion.IsCompletedSuccessfully, Is.True);
        }
    }

    [Test]
    public async Task EnqueueAsync_FullQueue_WaitsForCapacityWithoutWaitingForProcessing()
    {
        var entered = NewSignal();
        var release = NewSignal();
        var received = new List<int>();
        await using var queue = new MessageQueue<int>(async message =>
        {
            if (message == 1)
            {
                entered.SetResult();
                await release.Task;
            }

            received.Add(message);
        }, capacity: 1);

        try
        {
            await queue.EnqueueAsync(1);
            await entered.Task.WaitAsync(Timeout);
            Assert.That(queue.Completion.IsCompleted, Is.False);

            await queue.EnqueueAsync(2);
            Assert.That(queue.TryEnqueue(99), Is.False);

            var pending = queue.EnqueueAsync(3).AsTask();
            Assert.That(pending.IsCompleted, Is.False);

            release.SetResult();
            await pending.WaitAsync(Timeout);
            await queue.CompleteAsync().WaitAsync(Timeout);
            Assert.That(received, Is.EqualTo(new[] { 1, 2, 3 }));
        }
        finally
        {
            release.TrySetResult();
        }
    }

    [Test]
    public async Task EnqueueAsync_CancelWaitingWrite_DoesNotAdmitTheMessage()
    {
        var entered = NewSignal();
        var release = NewSignal();
        var received = new List<int>();
        await using var queue = new MessageQueue<int>(async message =>
        {
            if (message == 1)
            {
                entered.SetResult();
                await release.Task;
            }

            received.Add(message);
        }, capacity: 1);
        using var cancellation = new CancellationTokenSource();

        try
        {
            await queue.EnqueueAsync(1);
            await entered.Task.WaitAsync(Timeout);
            await queue.EnqueueAsync(2);
            var pending = queue.EnqueueAsync(3, cancellation.Token).AsTask();

            cancellation.Cancel();

            await Assert.ThatAsync(async () => await pending.WaitAsync(Timeout),
                Throws.InstanceOf<OperationCanceledException>());
            release.SetResult();
            await queue.CompleteAsync().WaitAsync(Timeout);
            Assert.That(received, Is.EqualTo(new[] { 1, 2 }));
        }
        finally
        {
            release.TrySetResult();
        }
    }

    [Test]
    public async Task Complete_RejectsWaitingAndFutureWritesButDrainsAcceptedMessages()
    {
        var entered = NewSignal();
        var release = NewSignal();
        var received = new List<int>();
        await using var queue = new MessageQueue<int>(async message =>
        {
            if (message == 1)
            {
                entered.SetResult();
                await release.Task;
            }

            received.Add(message);
        }, capacity: 1);

        try
        {
            await queue.EnqueueAsync(1);
            await entered.Task.WaitAsync(Timeout);
            await queue.EnqueueAsync(2);
            var pending = queue.EnqueueAsync(3).AsTask();

            queue.Complete();
            queue.Complete();

            Assert.That(queue.TryEnqueue(4), Is.False);
            await Assert.ThrowsAsync<ChannelClosedException>(async () => await pending.WaitAsync(Timeout));
            await Assert.ThrowsAsync<ChannelClosedException>(async () => await queue.EnqueueAsync(5));
            Assert.That(queue.Completion.IsCompleted, Is.False);

            release.SetResult();
            await queue.Completion.WaitAsync(Timeout);
            Assert.That(received, Is.EqualTo(new[] { 1, 2 }));
        }
        finally
        {
            release.TrySetResult();
        }
    }

    [Test]
    public async Task CompleteAsync_CancelWait_ContinuesDrainingTheQueue()
    {
        var entered = NewSignal();
        var release = NewSignal();
        var handled = false;
        await using var queue = new MessageQueue<int>(async _ =>
        {
            entered.SetResult();
            await release.Task;
            handled = true;
        });
        using var cancellation = new CancellationTokenSource();

        try
        {
            await queue.EnqueueAsync(1);
            await entered.Task.WaitAsync(Timeout);
            var stopping = queue.CompleteAsync(cancellation.Token);
            cancellation.Cancel();

            await Assert.ThatAsync(async () => await stopping.WaitAsync(Timeout),
                Throws.InstanceOf<OperationCanceledException>());
            Assert.That(queue.Completion.IsCompleted, Is.False);
            Assert.That(queue.TryEnqueue(2), Is.False);

            release.SetResult();
            await queue.CompleteAsync().WaitAsync(Timeout);
            Assert.That(handled, Is.True);
        }
        finally
        {
            release.TrySetResult();
        }
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task HandlerFailure_FaultsCompletionAndDisposeWithoutLogger(bool asynchronousFailure)
    {
        var failure = new InvalidOperationException("Handler failed.");
        var queue = new MessageQueue<int>(_ => asynchronousFailure
            ? ValueTask.FromException(failure)
            : throw failure);

        await queue.EnqueueAsync(1);
        var observed = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await queue.Completion.WaitAsync(Timeout));
        Assert.That(observed, Is.SameAs(failure));
        Assert.That(queue.TryEnqueue(2), Is.False);
        await Assert.ThrowsAsync<ChannelClosedException>(async () => await queue.EnqueueAsync(3));
        var disposed = await Assert.ThrowsAsync<InvalidOperationException>(async () => await queue.DisposeAsync());
        Assert.That(disposed, Is.SameAs(failure));
    }

    [Test]
    public async Task HandlerFailure_RejectsWaitingWriteAndDiscardsBufferedMessages()
    {
        var entered = NewSignal();
        var release = NewSignal();
        var received = new List<int>();
        var queue = new MessageQueue<int>(async message =>
        {
            received.Add(message);
            entered.SetResult();
            await release.Task;
            throw new InvalidOperationException("Handler failed.");
        }, capacity: 1);

        try
        {
            await queue.EnqueueAsync(1);
            await entered.Task.WaitAsync(Timeout);
            await queue.EnqueueAsync(2);
            var pending = queue.EnqueueAsync(3).AsTask();
            release.SetResult();

            await Assert.ThrowsAsync<InvalidOperationException>(async () => await queue.Completion.WaitAsync(Timeout));
            await Assert.ThrowsAsync<ChannelClosedException>(async () => await pending.WaitAsync(Timeout));
            Assert.That(received, Is.EqualTo(new[] { 1 }));
        }
        finally
        {
            release.TrySetResult();
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await queue.DisposeAsync());
        }
    }

    [Test]
    public async Task HandlerCancellation_CancelsCompletionAndClosesAdmission()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var queue = new MessageQueue<int>(_ => ValueTask.FromCanceled(cancellation.Token));

        await queue.EnqueueAsync(1);
        await Assert.ThatAsync(async () => await queue.Completion.WaitAsync(Timeout),
            Throws.InstanceOf<OperationCanceledException>());
        Assert.That(queue.Completion.IsCanceled, Is.True);
        Assert.That(queue.TryEnqueue(2), Is.False);
        await Assert.ThatAsync(async () => await queue.EnqueueAsync(3),
            Throws.InstanceOf<OperationCanceledException>());
        await Assert.ThatAsync(async () => await queue.DisposeAsync(),
            Throws.InstanceOf<OperationCanceledException>());
    }

    [Test]
    public async Task MulticastHandler_AwaitsEveryCallbackBeforeProcessingNextMessage()
    {
        var entered = NewSignal();
        var release = NewSignal();
        var received = new List<int>();
        Func<int, ValueTask> first = async message =>
        {
            if (message == 1)
            {
                entered.SetResult();
                await release.Task;
            }

            received.Add(message * 10 + 1);
        };
        Func<int, ValueTask> second = message =>
        {
            received.Add(message * 10 + 2);
            return ValueTask.CompletedTask;
        };
        await using var queue = new MessageQueue<int>(first + second);

        try
        {
            await queue.EnqueueAsync(1);
            await queue.EnqueueAsync(2);
            await entered.Task.WaitAsync(Timeout);
            Assert.That(received, Is.Empty);

            release.SetResult();
            await queue.CompleteAsync().WaitAsync(Timeout);
            Assert.That(received, Is.EqualTo(new[] { 11, 12, 21, 22 }));
        }
        finally
        {
            release.TrySetResult();
        }
    }

    [Test]
    public async Task ConcurrentProducers_EveryAcceptedMessageIsProcessedOnce()
    {
        var received = new ConcurrentDictionary<int, int>();
        await using var queue = new MessageQueue<int>(message =>
        {
            received.AddOrUpdate(message, 1, (_, count) => count + 1);
            return ValueTask.CompletedTask;
        }, capacity: 3);
        var producers = Enumerable.Range(0, 8).Select(producer => Task.Run(async () =>
        {
            for (var i = 0; i < 50; i++)
            {
                await queue.EnqueueAsync(producer * 50 + i);
            }
        }));

        await Task.WhenAll(producers).WaitAsync(Timeout);
        await queue.CompleteAsync().WaitAsync(Timeout);

        Assert.That(received, Has.Count.EqualTo(400));
        Assert.That(received.Values, Is.All.EqualTo(1));
    }

    [Test]
    public async Task ConcurrentDisposeAsync_AllCallersWaitForDrain()
    {
        var entered = NewSignal();
        var release = NewSignal();
        var queue = new MessageQueue<int>(async _ =>
        {
            entered.SetResult();
            await release.Task;
        });

        try
        {
            await queue.EnqueueAsync(1);
            await entered.Task.WaitAsync(Timeout);
            var disposals = Enumerable.Range(0, 8).Select(_ => queue.DisposeAsync().AsTask()).ToArray();

            Assert.That(disposals.All(task => !task.IsCompleted), Is.True);
            Assert.That(queue.TryEnqueue(2), Is.False);
            release.SetResult();
            await Task.WhenAll(disposals).WaitAsync(Timeout);
            await queue.DisposeAsync();
        }
        finally
        {
            release.TrySetResult();
            await queue.DisposeAsync();
        }
    }

    [Test]
    public async Task Completion_ReleasesStrongHandlerReference()
    {
        var (queue, target) = CreateQueueWithCollectableHandler();

        Collect();
        Assert.That(target.IsAlive, Is.True);
        await queue.CompleteAsync().WaitAsync(Timeout);
        Collect();

        Assert.That(target.IsAlive, Is.False);
        await queue.DisposeAsync();
        GC.KeepAlive(queue);
    }

    [Test]
    public async Task HandlerFailure_ReleasesDiscardedBufferedPayload()
    {
        var entered = NewSignal();
        var release = NewSignal();
        var queue = new MessageQueue<object>(async _ =>
        {
            entered.SetResult();
            await release.Task;
            throw new InvalidOperationException("Handler failed.");
        }, capacity: 1);

        try
        {
            await queue.EnqueueAsync(new object());
            await entered.Task.WaitAsync(Timeout);
            var buffered = EnqueueCollectablePayload(queue);
            Collect();
            Assert.That(buffered.IsAlive, Is.True);

            release.SetResult();
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await queue.Completion.WaitAsync(Timeout));
            Collect();
            Assert.That(buffered.IsAlive, Is.False);
            GC.KeepAlive(queue);
        }
        finally
        {
            release.TrySetResult();
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await queue.DisposeAsync());
        }
    }

    private static TaskCompletionSource NewSignal() => new(TaskCreationOptions.RunContinuationsAsynchronously);

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static (MessageQueue<int> Queue, WeakReference Target) CreateQueueWithCollectableHandler()
    {
        var handler = new QueueHandler();
        return (new MessageQueue<int>(handler.HandleAsync), new WeakReference(handler));
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference EnqueueCollectablePayload(MessageQueue<object> queue)
    {
        var message = new object();
        Assert.That(queue.TryEnqueue(message), Is.True);
        return new WeakReference(message);
    }

    private static void Collect()
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
    }

    private sealed class QueueHandler
    {
        internal ValueTask HandleAsync(int message) => ValueTask.CompletedTask;
    }
}
