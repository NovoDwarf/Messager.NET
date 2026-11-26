using System.Diagnostics.CodeAnalysis;
using Messager.NET.Entities;

namespace Messager.NET.Tests.Entities.UnsubscribersAsync;

[TestFixture]
public sealed partial class UnsubscriberAsync_Tests
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
    [SuppressMessage("Reliability", "CA2012")]
    public void IsDisposed_ShouldReflectCorrectState()
    {
        var unsubscriber = new UnsubscriberAsync(() => ValueTask.CompletedTask);

        Assert.That(unsubscriber.IsDisposed, Is.False);

        unsubscriber.DisposeAsync().GetAwaiter().GetResult();
        Assert.That(unsubscriber.IsDisposed, Is.True);
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
    public async Task ConfigureAwaitFalse_ShouldNotCaptureContext()
    {
        var synchronizationContext = new TestSynchronizationContext();
        var originalContext = SynchronizationContext.Current;
        var completion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var callbackContext = default(SynchronizationContext);
        var unsubscriber = new UnsubscriberAsync(() => { callbackContext = SynchronizationContext.Current; return new ValueTask(completion.Task);
    });
        ValueTask disposal;

        try
        {
            SynchronizationContext.SetSynchronizationContext(synchronizationContext);
            disposal = unsubscriber.DisposeAsync();
        }
        finally
        {
            SynchronizationContext.SetSynchronizationContext(originalContext);
        }

        completion.SetResult();
        await disposal;

        using (Assert.EnterMultipleScope())
        {
            Assert.That(callbackContext, Is.SameAs(synchronizationContext));
            Assert.That(synchronizationContext.PostCount, Is.Zero);
        }
    }

    private sealed class TestSynchronizationContext : SynchronizationContext
    {
        private int _postCount;

        public int PostCount => Volatile.Read(ref _postCount);

        public override void Post(SendOrPostCallback d, object? state)
        {
            Interlocked.Increment(ref _postCount);
            d(state);
        }
    }
}
