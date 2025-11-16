using Messager.NET.Models.Resources;
using Messager.NET.Tests.Objects;
using Messager.NET.Tests.Objects.Resources;

namespace Messager.NET.Tests.Models.Resources;

public partial class DisposableListTests
{
    [Test]
    public void Dispose_ShouldClearInternalList()
    {
        var disposableList = new DisposableList();
        disposableList.Add(new TestDisposable(), new TestDisposable());

        disposableList.Dispose();

        Assert.That(disposableList.Count, Is.Zero);
    }
    
    [Test]
    public void Dispose_ShouldDisposeAllContainedDisposables()
    {
        var disposableList = new DisposableList();
        var disposable1 = new TestDisposable();
        var disposable2 = new TestDisposable();
        var disposable3 = new TestDisposable();

        disposableList.Add(disposable1, disposable2, disposable3);

        disposableList.Dispose();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(disposable1.IsDisposed, Is.True);
            Assert.That(disposable2.IsDisposed, Is.True);
            Assert.That(disposable3.IsDisposed, Is.True);
            Assert.That(disposableList.IsDisposed, Is.True);
            Assert.That(disposableList.Count, Is.Zero);
        }
    }

    [Test]
    public void Dispose_ShouldBeIdempotent()
    {
        var disposableList = new DisposableList();
        var disposable = new TestDisposable();
        disposableList.Add(disposable);

        disposableList.Dispose();
        disposableList.Dispose();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(disposable.DisposeCount, Is.EqualTo(1));
            Assert.That(disposableList.IsDisposed, Is.True);
        }
    }

    [Test]
    public void Dispose_WithThrowingDisposables_ShouldThrowAggregateException()
    {
        var disposableList = new DisposableList();
        var goodDisposable = new TestDisposable();
        var badDisposable = new ThrowingDisposable();
        var anotherBadDisposable = new ThrowingDisposable("Second error");

        disposableList.Add(goodDisposable, badDisposable, anotherBadDisposable);

        var exception = Assert.Throws<AggregateException>(() => disposableList.Dispose());
        
        using (Assert.EnterMultipleScope())
        {
            Assert.That(goodDisposable.IsDisposed, Is.True);
            Assert.That(badDisposable.IsDisposed, Is.True);
            Assert.That(anotherBadDisposable.IsDisposed, Is.True);

            Assert.That(exception.Message, Does.Contain("One or more disposables threw exceptions during disposal"));
            Assert.That(exception.InnerExceptions, Has.Count.EqualTo(1));
            Assert.That(exception.InnerExceptions[0].Message, Is.EqualTo("Test exception"));
        }
    }
}