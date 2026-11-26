using System.Collections.Concurrent;
using Messager.NET.Queues;
using static Messager.NET.Tests.Helpers.TestHelpers;

namespace Messager.NET.Tests.Queues.MessageQueue;

public sealed partial class MessageQueue_Tests
{
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
            await entered.Task.WaitAsync(DefaultTimeout);
            Assert.That(received, Is.Empty);

            release.SetResult();
            await queue.CompleteAsync().WaitAsync(DefaultTimeout);
            Assert.That(received, Is.EqualTo([11, 12, 21, 22]));
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
            // ReSharper disable once AccessToDisposedClosure
            for (var i = 0; i < 50; i++)
                await queue.EnqueueAsync(producer * 50 + i);
        }));

        await Task.WhenAll(producers).WaitAsync(DefaultTimeout);
        await queue.CompleteAsync().WaitAsync(DefaultTimeout);

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
            await entered.Task.WaitAsync(DefaultTimeout);

            var disposals = Enumerable.Range(0, 8).Select(_ => queue.DisposeAsync().AsTask()).ToArray();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(disposals.All(task => !task.IsCompleted), Is.True);
                Assert.That(queue.TryEnqueue(2), Is.False);
            }

            release.SetResult();

            await Task.WhenAll(disposals).WaitAsync(DefaultTimeout);
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

        CollectGarbage();
        Assert.That(target.IsAlive, Is.True);
        await queue.CompleteAsync().WaitAsync(DefaultTimeout);
        CollectGarbage();

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
            await entered.Task.WaitAsync(DefaultTimeout);

            var buffered = EnqueueCollectablePayload(queue);

            CollectGarbage();
            Assert.That(buffered.IsAlive, Is.True);

            release.SetResult();
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await queue.Completion.WaitAsync(DefaultTimeout));

            CollectGarbage();
            Assert.That(buffered.IsAlive, Is.False);
            GC.KeepAlive(queue);
        }
        finally
        {
            release.TrySetResult();
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await queue.DisposeAsync());
        }
    }
}
