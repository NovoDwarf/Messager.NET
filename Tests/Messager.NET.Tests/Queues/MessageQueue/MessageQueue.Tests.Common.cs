using System.Diagnostics.CodeAnalysis;
using System.Threading.Channels;
using Messager.NET.Queues;
using static Messager.NET.Tests.Helpers.TestHelpers;

namespace Messager.NET.Tests.Queues.MessageQueue;

[TestFixture]
public sealed partial class MessageQueue_Tests
{

    [TestCase(0)]
    [TestCase(-1)]
    public void Constructor_InvalidCapacity_Throws(int capacity)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            _ = new MessageQueue<int>(_ => ValueTask.CompletedTask, capacity);
        });
    }

    [Test]
    public void Constructor_NullHandler_Throws()
    {
        Assert.Throws<ArgumentNullException>(() =>
        {
            _ = new MessageQueue<int>(null!);
        });
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
            await queue.EnqueueAsync(i);

        await queue.CompleteAsync().WaitAsync(DefaultTimeout);

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
            await entered.Task.WaitAsync(DefaultTimeout);
            Assert.That(queue.Completion.IsCompleted, Is.False);

            await queue.EnqueueAsync(2);
            Assert.That(queue.TryEnqueue(99), Is.False);

            var pending = queue.EnqueueAsync(3).AsTask();
            Assert.That(pending.IsCompleted, Is.False);

            release.SetResult();
            await pending.WaitAsync(DefaultTimeout);
            await queue.CompleteAsync().WaitAsync(DefaultTimeout);
            Assert.That(received, Is.EqualTo([1, 2, 3]));
        }
        finally
        {
            release.TrySetResult();
        }
    }

    [Test]
    [SuppressMessage("ReSharper", "MethodSupportsCancellation")]
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
            await queue.EnqueueAsync(1, cancellation.Token);
            await entered.Task.WaitAsync(DefaultTimeout, cancellation.Token);
            await queue.EnqueueAsync(2, cancellation.Token);

            var pending = queue.EnqueueAsync(3, cancellation.Token).AsTask();

            await cancellation.CancelAsync();

            await Assert.ThatAsync(async () => await pending.WaitAsync(DefaultTimeout), Throws.InstanceOf<OperationCanceledException>());
            release.SetResult();
            await queue.CompleteAsync().WaitAsync(DefaultTimeout);
            Assert.That(received, Is.EqualTo([1, 2]));
        }
        finally
        {
            release.TrySetResult();
        }
    }

    [Test]
    [SuppressMessage("ReSharper", "MethodHasAsyncOverload")]
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
            await entered.Task.WaitAsync(DefaultTimeout);
            await queue.EnqueueAsync(2);
            var pending = queue.EnqueueAsync(3).AsTask();

            queue.Complete();
            queue.Complete();

            Assert.That(queue.TryEnqueue(4), Is.False);
            await Assert.ThrowsAsync<ChannelClosedException>(async () => await pending.WaitAsync(DefaultTimeout));
            await Assert.ThrowsAsync<ChannelClosedException>(async () => await queue.EnqueueAsync(5));
            Assert.That(queue.Completion.IsCompleted, Is.False);

            release.SetResult();
            await queue.Completion.WaitAsync(DefaultTimeout);
            Assert.That(received, Is.EqualTo([1, 2]));
        }
        finally
        {
            release.TrySetResult();
        }
    }

    [Test]
    [SuppressMessage("ReSharper", "MethodSupportsCancellation")]
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
            await entered.Task.WaitAsync(DefaultTimeout);
            var stopping = queue.CompleteAsync(cancellation.Token);
            await cancellation.CancelAsync();

            await Assert.ThatAsync(async () => await stopping.WaitAsync(DefaultTimeout), Throws.InstanceOf<OperationCanceledException>());

            using (Assert.EnterMultipleScope())
            {
                Assert.That(queue.Completion.IsCompleted, Is.False);
                Assert.That(queue.TryEnqueue(2), Is.False);
            }

            release.SetResult();
            await queue.CompleteAsync().WaitAsync(DefaultTimeout);
            Assert.That(handled, Is.True);
        }
        finally
        {
            release.TrySetResult();
        }
    }
}
