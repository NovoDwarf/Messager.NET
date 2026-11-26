using Messager.NET.Entities;

namespace Messager.NET.Tests.Entities.UnsubscribersAsync;

public sealed partial class UnsubscriberAsync_Tests
{
    [Test]
    public async Task ThreadSafety_MultipleThreadsCallingDisposeAsync_ShouldExecuteFuncOnlyOnce()
    {
        var callCount = 0;
        var unsubscriber = new UnsubscriberAsync(() =>
        {
            Interlocked.Increment(ref callCount);
            return ValueTask.CompletedTask;
        });

        const int taskCount = 10;
        var tasks = new Task[taskCount];

        for (var i = 0; i < taskCount; i++)
            tasks[i] = Task.Run(async () => await unsubscriber.DisposeAsync());

        await Task.WhenAll(tasks);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(callCount, Is.EqualTo(1));
            Assert.That(unsubscriber.IsDisposed, Is.True);
        }
    }

    [Test]
    public async Task ThreadSafety_ConcurrentDisposeAsyncCalls_ShouldBeThreadSafe()
    {
        var callCount = 0;
        var unsubscriber = new UnsubscriberAsync(() =>
        {
            Interlocked.Increment(ref callCount);
            return ValueTask.CompletedTask;
        });

        const int taskCount = 100;
        var tasks = new Task[taskCount];

        for (var i = 0; i < taskCount; i++)
            tasks[i] = Task.Run(async () => await unsubscriber.DisposeAsync());

        await Task.WhenAll(tasks);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(callCount, Is.EqualTo(1));
            Assert.That(unsubscriber.IsDisposed, Is.True);
        }
    }

    [Test]
    public async Task InterlockedCompareExchange_ShouldEnsureAtomicOperation()
    {
        var callCount = 0;
        var unsubscriber = new UnsubscriberAsync(() => { callCount++; return ValueTask.CompletedTask; });

        var task1 = Task.Run(async () =>
        {
            for (var i = 0; i < 100; i++)
            {
                await unsubscriber.DisposeAsync();
                await Task.Yield();
            }
        });

        var task2 = Task.Run(async () =>
        {
            for (var i = 0; i < 100; i++)
            {
                await unsubscriber.DisposeAsync();
                await Task.Yield();
            }
        });

        await Task.WhenAll(task1, task2);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(callCount, Is.EqualTo(1));
            Assert.That(unsubscriber.IsDisposed, Is.True);
        }
    }

    [Test]
    public async Task StressTest_HighConcurrency_ShouldRemainThreadSafe()
    {
        const int iterations = 100;

        for (var i = 0; i < iterations; i++)
        {
            var callCount = 0;
            var unsubscriber = new UnsubscriberAsync(() =>
            {
                Interlocked.Increment(ref callCount);
                return ValueTask.CompletedTask;
            });

            var tasks = new Task[Environment.ProcessorCount * 2];

            for (var j = 0; j < tasks.Length; j++)
                tasks[j] = Task.Run(async () => await unsubscriber.DisposeAsync());

            await Task.WhenAll(tasks);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(callCount, Is.EqualTo(1), $"Iteration {i} failed");
                Assert.That(unsubscriber.IsDisposed, Is.True, $"Iteration {i} failed");
            }
        }
    }
}
