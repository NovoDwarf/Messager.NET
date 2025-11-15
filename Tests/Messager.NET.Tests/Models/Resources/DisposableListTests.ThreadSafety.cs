using Messager.NET.Models.Resources;
using Messager.NET.Tests.Objects;
using Messager.NET.Tests.Objects.Resources;

namespace Messager.NET.Tests.Models.Resources;

public partial class DisposableListTests
{
	[Test]
    public void ThreadSafety_MultipleThreadsAdding_ShouldNotCorruptState()
    {
        const int threadCount = 10;
        const int disposablesPerThread = 100;
        
        var disposableList = new DisposableList();
        var threads = new List<Thread>();

        for (var i = 0; i < threadCount; i++)
        {
            var thread = new Thread(() =>
            {
                for (var j = 0; j < disposablesPerThread; j++)
                {
                    disposableList.Add(new TestDisposable());
                }
            });
            threads.Add(thread);
            thread.Start();
        }

        foreach (var thread in threads) 
            thread.Join();

        Assert.That(disposableList.Count, Is.EqualTo(threadCount * disposablesPerThread));
    }

    [Test]
    public void ThreadSafety_ConcurrentAddAndDispose_ShouldNotThrowOrCorrupt()
    {
        var disposableList = new DisposableList();
        var addThread = new Thread(() =>
        {
            for (var i = 0; i < 1000; i++)
            {
                try
                {
                    disposableList.Add(new TestDisposable());
                }
                catch (ObjectDisposedException)
                {
                    break;
                }
            }
        });

        addThread.Start();
        Thread.Sleep(10);
        disposableList.Dispose();
        addThread.Join();

        Assert.That(disposableList.IsDisposed, Is.True);
    }

    [Test]
    public async Task ThreadSafety_AsyncOperations_ShouldBeThreadSafe()
    {
        var disposableList = new DisposableList();
        var tasks = new List<Task>();

        for (var i = 0; i < 10; i++)
        {
            tasks.Add(Task.Run(() =>
            {
                for (var j = 0; j < 100; j++)
                {
                    disposableList.Add(new TestDisposable());
                }
            }));
        }

        await Task.WhenAll(tasks);

        Assert.That(disposableList.Count, Is.EqualTo(1000));
    }
}