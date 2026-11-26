using Messager.NET.Entities;

namespace Messager.NET.Tests.Entities.Unsubscribers;

public sealed partial class Unsubscriber_Tests
{
    [Test]
    public void ThreadSafety_MultipleThreadsCallingDispose_ShouldExecuteActionOnlyOnce()
    {
        const int threadCount = 10;

        var callCount = 0;
        var unsubscriber = new Unsubscriber(() => Interlocked.Increment(ref callCount));
        var threads = new Thread[threadCount];

        for (var i = 0; i < threadCount; i++)
        {
            threads[i] = new Thread(() =>
            {
                try
                {
                    unsubscriber.Dispose();
                }
                catch { }
            });
        }

        foreach (var thread in threads)
            thread.Start();

        foreach (var thread in threads)
            thread.Join();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(callCount, Is.EqualTo(1)); // Executed exactly once
            Assert.That(unsubscriber.IsDisposed, Is.True);
        }
    }

    [Test]
    public async Task ThreadSafety_ConcurrentDisposeCalls_ShouldBeThreadSafe()
    {
        var callCount = 0;
        var unsubscriber = new Unsubscriber(() => Interlocked.Increment(ref callCount));
        const int taskCount = 100;
        var tasks = new Task[taskCount];

        for (var i = 0; i < taskCount; i++)
            tasks[i] = Task.Run(unsubscriber.Dispose);

        await Task.WhenAll(tasks);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(callCount, Is.EqualTo(1));
            Assert.That(unsubscriber.IsDisposed, Is.True);
        }
    }

    [Test]
    public void InterlockedExchange_ShouldEnsureAtomicOperation()
    {
        var callCount = 0;
        var unsubscriber = new Unsubscriber(() => callCount++);

        var task1 = Task.Run(() =>
        {
            for (var i = 0; i < 1000; i++)
            {
                unsubscriber.Dispose();
                Thread.Yield();
            }
        });

        var task2 = Task.Run(() =>
        {
            for (var i = 0; i < 1000; i++)
            {
                unsubscriber.Dispose();
                Thread.Yield();
            }
        });

        Task.WaitAll(task1, task2);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(callCount, Is.EqualTo(1));
            Assert.That(unsubscriber.IsDisposed, Is.True);
        }
    }

    [Test]
    public void StressTest_HighConcurrency_ShouldRemainThreadSafe()
    {
        const int iterations = 1000;

        for (var i = 0; i < iterations; i++)
        {
            var callCount = 0;
            var unsubscriber = new Unsubscriber(() => Interlocked.Increment(ref callCount));
            var tasks = new Task[Environment.ProcessorCount * 2];

            for (var j = 0; j < tasks.Length; j++)
                tasks[j] = Task.Run(unsubscriber.Dispose);

            Task.WaitAll(tasks);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(callCount, Is.EqualTo(1), $"Iteration {i} failed");
                Assert.That(unsubscriber.IsDisposed, Is.True, $"Iteration {i} failed");
            }
        }
    }
}
