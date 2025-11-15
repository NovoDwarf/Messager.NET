using Messager.NET.Models.Resources;
using Messager.NET.Tests.Objects;
using Messager.NET.Tests.Objects.Resources;

namespace Messager.NET.Tests.Models.Resources;

public partial class DisposableSmartTests
{
	[Test]
	public void ThreadSafety_MultipleThreadsCallingDispose_ShouldDisposeOnlyOnce()
	{
		const int threadCount = 10;

		var disposable = new TestDisposable();
		var disposableSmart = new DisposableSmart(disposable);
		var threads = new Thread[threadCount];
		var disposeCount = 0;

		for (var i = 0; i < threadCount; i++)
		{
			threads[i] = new Thread(() =>
			{
				try
				{
					disposableSmart.Dispose();
					Interlocked.Increment(ref disposeCount);
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
			Assert.That(disposable.DisposeCount, Is.EqualTo(1));
			Assert.That(disposeCount, Is.EqualTo(threadCount));
			Assert.That(disposableSmart.IsDisposed, Is.True);
		}
	}

	[Test]
	public async Task ThreadSafety_ConcurrentDisposeCalls_ShouldBeThreadSafe()
	{
		const int taskCount = 100;

		var disposable = new TestDisposable();
		var disposableSmart = new DisposableSmart(disposable);
		var tasks = new Task[taskCount];

		for (var i = 0; i < taskCount; i++)
			tasks[i] = Task.Run(() => disposableSmart.Dispose());

		await Task.WhenAll(tasks);

		using (Assert.EnterMultipleScope())
		{
			Assert.That(disposable.DisposeCount, Is.EqualTo(1));
			Assert.That(disposableSmart.IsDisposed, Is.True);
		}
	}
}