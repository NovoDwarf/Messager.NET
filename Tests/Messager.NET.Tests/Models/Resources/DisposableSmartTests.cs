using Messager.NET.Models.Resources;
using Messager.NET.Tests.Objects;
using Messager.NET.Tests.Objects.Resources;

namespace Messager.NET.Tests.Models.Resources;

[TestFixture]
public partial class DisposableSmartTests
{
    [Test]
    public void Constructor_WithValidDisposable_ShouldInitialize()
    {
        var disposable = new TestDisposable();

        var disposableSmart = new DisposableSmart(disposable);

        Assert.That(disposableSmart.IsDisposed, Is.False);
    }

    [Test]
    public void Constructor_WithNullDisposable_ShouldThrowArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
        {
            var smart = new DisposableSmart(null!);
        });
    }

    [Test]
    public void IsDisposed_ShouldReflectCorrectState()
    {
        var disposable = new TestDisposable();
        var disposableSmart = new DisposableSmart(disposable);

        Assert.That(disposableSmart.IsDisposed, Is.False);

        disposableSmart.Dispose();
        Assert.That(disposableSmart.IsDisposed, Is.True);
    }

    [Test]
    public void MultipleInstances_ShouldWorkIndependently()
    {
        var disposable1 = new TestDisposable();
        var disposable2 = new TestDisposable();
        var smart1 = new DisposableSmart(disposable1);
        var smart2 = new DisposableSmart(disposable2);

        smart1.Dispose();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(smart1.IsDisposed, Is.True);
            Assert.That(disposable1.IsDisposed, Is.True);
            Assert.That(smart2.IsDisposed, Is.False);
            Assert.That(disposable2.IsDisposed, Is.False);
        }
    }

    [Test]
    public void VolatileRead_ShouldEnsureMemoryVisibility()
    {
        var disposable = new TestDisposable();
        var disposableSmart = new DisposableSmart(disposable);
        
        var task1 = Task.Run(() =>
        {
            for (var i = 0; i < 1000; i++)
            {
                if (disposableSmart.IsDisposed)
                    break;
                
                Thread.Yield();
            }
        });

        var task2 = Task.Run(() =>
        {
            Thread.Sleep(10);
            disposableSmart.Dispose();
        });

        Task.WaitAll(task1, task2);

        Assert.That(disposableSmart.IsDisposed, Is.True);
    }
}