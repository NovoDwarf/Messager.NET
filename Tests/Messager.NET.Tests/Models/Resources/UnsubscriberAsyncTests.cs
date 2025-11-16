using Messager.NET.Models.Resources;

namespace Messager.NET.Tests.Models.Resources;

using NUnit.Framework;
using System;
using System.Threading;
using System.Threading.Tasks;

[TestFixture]
public partial class UnsubscriberAsyncTests
{
	[Test]
	public void Constructor_WithValidFunc_ShouldInitialize()
	{
		var func = new Func<ValueTask>(() => ValueTask.CompletedTask);
		
		var unsubscriber = new UnsubscriberAsync(func);

		Assert.That(unsubscriber.IsDisposed, Is.False);
	}

	[Test]
	public void Constructor_WithNullFunc_ShouldThrowArgumentNullException()
	{
		Assert.Throws<ArgumentNullException>(() =>
		{
			var unsubscriberAsync = new UnsubscriberAsync(null!);
		});
	}

	[Test]
	public void IsDisposed_ShouldReflectCorrectState()
	{
		var unsubscriber = new UnsubscriberAsync(() => ValueTask.CompletedTask);

		Assert.That(unsubscriber.IsDisposed, Is.False);

		unsubscriber.DisposeAsync().GetAwaiter().GetResult();
		Assert.That(unsubscriber.IsDisposed, Is.True);
	}

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
	public async Task MultipleInstances_ShouldWorkIndependently()
	{
		var callCount1 = 0;
		var callCount2 = 0;
		
		var unsubscriber1 = new UnsubscriberAsync(() =>
		{
			callCount1++;
			return ValueTask.CompletedTask;
		});
		
		var unsubscriber2 = new UnsubscriberAsync(() =>
		{
			callCount2++;
			return ValueTask.CompletedTask;
		});

		await unsubscriber1.DisposeAsync();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(unsubscriber1.IsDisposed, Is.True);
            Assert.That(callCount1, Is.EqualTo(1));
            Assert.That(unsubscriber2.IsDisposed, Is.False);
            Assert.That(callCount2, Is.Zero);
        }
    }

	[Test]
	public async Task InterlockedCompareExchange_ShouldEnsureAtomicOperation()
	{
		var callCount = 0;
		var unsubscriber = new UnsubscriberAsync(() =>
		{
			callCount++;
			return ValueTask.CompletedTask;
		});

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
			{
				tasks[j] = Task.Run(async () => await unsubscriber.DisposeAsync());
			}

			await Task.WhenAll(tasks);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(callCount, Is.EqualTo(1), $"Iteration {i} failed");
                Assert.That(unsubscriber.IsDisposed, Is.True, $"Iteration {i} failed");
            }
        }
	}

	[Test]
	public async Task ConfigureAwaitFalse_ShouldNotCaptureContext()
	{
		var synchronizationContext = new TestSynchronizationContext();
		SynchronizationContext.SetSynchronizationContext(synchronizationContext);

		var contextCaptured = false;
		var unsubscriber = new UnsubscriberAsync(async () =>
		{
			contextCaptured = SynchronizationContext.Current == synchronizationContext;
			await Task.Delay(10);
		});

		try
		{
			await unsubscriber.DisposeAsync();

			Assert.That(contextCaptured, Is.False);
		}
		finally
		{
			SynchronizationContext.SetSynchronizationContext(null);
		}
	}
	
	private class TestSynchronizationContext : SynchronizationContext
	{
		public override void Post(SendOrPostCallback d, object? state)
		{
			// Simulate context behavior
			d(state);
		}
	}
}

