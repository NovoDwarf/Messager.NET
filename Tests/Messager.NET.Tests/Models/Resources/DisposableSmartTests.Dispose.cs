using Messager.NET.Models.Resources;
using Messager.NET.Tests.Objects;
using Messager.NET.Tests.Objects.Resources;

namespace Messager.NET.Tests.Models.Resources;

public partial class DisposableSmartTests
{
	[Test]
	public void Dispose_ShouldDisposeUnderlyingDisposable()
	{
		var disposable = new TestDisposable();
		var disposableSmart = new DisposableSmart(disposable);

		disposableSmart.Dispose();

		using (Assert.EnterMultipleScope())
		{
			Assert.That(disposable.IsDisposed, Is.True);
			Assert.That(disposableSmart.IsDisposed, Is.True);
		}
	}

	[Test]
	public void Dispose_ShouldBeIdempotent()
	{
		var disposable = new TestDisposable();
		var disposableSmart = new DisposableSmart(disposable);

		disposableSmart.Dispose();
		disposableSmart.Dispose();
		disposableSmart.Dispose();

		using (Assert.EnterMultipleScope())
		{
			Assert.That(disposable.DisposeCount, Is.EqualTo(1));
			Assert.That(disposableSmart.IsDisposed, Is.True);
		}
	}

	[Test]
	public void Dispose_WithThrowingDisposable_ShouldPropagateException()
	{
		var throwingDisposable = new ThrowingDisposable();
		var disposableSmart = new DisposableSmart(throwingDisposable);

		Assert.Throws<InvalidOperationException>(() => disposableSmart.Dispose());

		using (Assert.EnterMultipleScope())
		{
			Assert.That(disposableSmart.IsDisposed, Is.True);
			Assert.That(throwingDisposable.IsDisposed, Is.True);
		}
	}

	[Test]
	public void Dispose_ShouldSetIsDisposedImmediately()
	{
		// Arrange
		var disposable = new SlowDisposable(TimeSpan.FromMilliseconds(100));
		var disposableSmart = new DisposableSmart(disposable);

		// Act
		var disposeTask = Task.Run(() => disposableSmart.Dispose());

		// Give it a moment to start disposing
		Thread.Sleep(10);

		// Assert - IsDisposed should be true even before the underlying disposable finishes
		Assert.That(disposableSmart.IsDisposed, Is.True);

		// Wait for completion to avoid test cleanup issues
		disposeTask.Wait();
	}

	[Test]
	public void Dispose_WhenUnderlyingDisposableIsAlsoSmartDisposable_ShouldWorkCorrectly()
	{
		// Arrange
		var innerDisposable = new TestDisposable();
		var innerSmart = new DisposableSmart(innerDisposable);
		var outerSmart = new DisposableSmart(innerSmart);

		// Act
		outerSmart.Dispose();

		// Assert
		Assert.That(outerSmart.IsDisposed, Is.True);
		Assert.That(innerSmart.IsDisposed, Is.True);
		Assert.That(innerDisposable.IsDisposed, Is.True);
		Assert.That(innerDisposable.DisposeCount, Is.EqualTo(1));
	}
}