using Messager.NET.Models.Resources;

namespace Messager.NET.Tests.Models.Resources;

using NUnit.Framework;
using System;
using System.Threading;
using System.Threading.Tasks;

[TestFixture]
public partial class UnsubscriberTests
{
    [Test]
    public void Constructor_WithValidAction_ShouldInitialize()
    {
        var action = new Action(() => { });

        var unsubscriber = new Unsubscriber(action);

        Assert.That(unsubscriber.IsDisposed, Is.False);
    }

    [Test]
    public void Constructor_WithNullAction_ShouldThrowArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
        {
            var unsubscriber = new Unsubscriber(null!);
        });
    }

    [Test]
    public void IsDisposed_ShouldReflectCorrectState()
    {
        var unsubscriber = new Unsubscriber(() => { });

        Assert.That(unsubscriber.IsDisposed, Is.False);

        unsubscriber.Dispose();
        Assert.That(unsubscriber.IsDisposed, Is.True);
    }

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
                catch
                {
                }
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
            tasks[i] = Task.Run(() => unsubscriber.Dispose());

        await Task.WhenAll(tasks);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(callCount, Is.EqualTo(1));
            Assert.That(unsubscriber.IsDisposed, Is.True);
        }
    }

    [Test]
    public void MultipleInstances_ShouldWorkIndependently()
    {
        var callCount1 = 0;
        var callCount2 = 0;
        
        var unsubscriber1 = new Unsubscriber(() => callCount1++);
        var unsubscriber2 = new Unsubscriber(() => callCount2++);

        unsubscriber1.Dispose();
        
        using (Assert.EnterMultipleScope())
        {
            Assert.That(unsubscriber1.IsDisposed, Is.True);
            Assert.That(callCount1, Is.EqualTo(1));
            Assert.That(unsubscriber2.IsDisposed, Is.False);
            Assert.That(callCount2, Is.Zero);
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
                tasks[j] = Task.Run(() => unsubscriber.Dispose());

            Task.WaitAll(tasks);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(callCount, Is.EqualTo(1), $"Iteration {i} failed");
                Assert.That(unsubscriber.IsDisposed, Is.True, $"Iteration {i} failed");
            }
        }
    }
}