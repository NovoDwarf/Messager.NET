using Messager.NET.Models.Resources;

namespace Messager.NET.Tests.Models.Resources;

public partial class UnsubscriberTests
{
	[Test]
	public void Dispose_ShouldExecuteAction()
	{
		var wasCalled = false;
		var unsubscriber = new Unsubscriber(() => wasCalled = true);

		unsubscriber.Dispose();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(wasCalled, Is.True);
            Assert.That(unsubscriber.IsDisposed, Is.True);
        }
    }

	[Test]
	public void Dispose_ShouldBeIdempotent()
	{
		var callCount = 0;
		var unsubscriber = new Unsubscriber(() => callCount++);

		unsubscriber.Dispose();
		unsubscriber.Dispose();
		unsubscriber.Dispose();

        using (Assert.EnterMultipleScope())
        {
	        Assert.That(callCount, Is.EqualTo(1));
            Assert.That(unsubscriber.IsDisposed, Is.True);
        }
    }

	[Test]
	public void Dispose_WithThrowingAction_ShouldPropagateException()
	{
		var unsubscriber = new Unsubscriber(() => throw new InvalidOperationException("Test exception"));

		Assert.Throws<InvalidOperationException>(() => unsubscriber.Dispose());
		Assert.That(unsubscriber.IsDisposed, Is.True);
	}

	[Test]
	public void Dispose_WithThrowingAction_ShouldStillMarkAsDisposed()
	{
		var unsubscriber = new Unsubscriber(() => throw new InvalidOperationException("Test exception"));

		try
		{
			unsubscriber.Dispose();
		}
		catch (InvalidOperationException)
		{
		}

		Assert.That(unsubscriber.IsDisposed, Is.True);
	}

	[Test]
	public void Dispose_ShouldSetIsDisposedImmediately()
	{
		var actionExecuted = false;
		var unsubscriber = new Unsubscriber(() =>
		{
			Thread.Sleep(100);
			actionExecuted = true;
		});

		var disposeTask = Task.Run(() => unsubscriber.Dispose());

		Thread.Sleep(10);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(unsubscriber.IsDisposed, Is.True);
            Assert.That(actionExecuted, Is.False);
        }

        disposeTask.Wait();
		Assert.That(actionExecuted, Is.True);
	}

	[Test]
	public void Dispose_ShouldWorkWithComplexAction()
	{
		var value1 = 0;
		var value2 = string.Empty;
		
		var unsubscriber = new Unsubscriber(() =>
		{
			value1 = 42;
			value2 = " ";
		});

		unsubscriber.Dispose();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(value1, Is.EqualTo(42));
            Assert.That(value2, Is.EqualTo(" "));
            Assert.That(unsubscriber.IsDisposed, Is.True);
        }
    }

	[Test]
	public void Dispose_AfterException_ShouldRemainIdempotent()
	{
		var callCount = 0;
		var unsubscriber = new Unsubscriber(() =>
		{
			callCount++;
			
			if (callCount == 1)
				throw new InvalidOperationException("First call fails");
		});

		Assert.Throws<InvalidOperationException>(() => unsubscriber.Dispose());

		Assert.DoesNotThrow(() => unsubscriber.Dispose());
		Assert.DoesNotThrow(() => unsubscriber.Dispose());

        using (Assert.EnterMultipleScope())
        {
            Assert.That(callCount, Is.EqualTo(1));
            Assert.That(unsubscriber.IsDisposed, Is.True);
        }
    }
}