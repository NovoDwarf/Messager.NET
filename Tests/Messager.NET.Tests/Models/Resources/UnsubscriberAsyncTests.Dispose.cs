using Messager.NET.Models.Resources;

namespace Messager.NET.Tests.Models.Resources;

public partial class UnsubscriberAsyncTests
{
	[Test]
	public async Task DisposeAsync_ShouldExecuteFunc()
	{
		var wasCalled = false;
		var unsubscriber = new UnsubscriberAsync(() =>
		{
			wasCalled = true;
			return ValueTask.CompletedTask;
		});

		await unsubscriber.DisposeAsync();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(wasCalled, Is.True);
            Assert.That(unsubscriber.IsDisposed, Is.True);
        }
    }

	[Test]
	public async Task DisposeAsync_ShouldBeIdempotent()
	{
		var callCount = 0;
		var unsubscriber = new UnsubscriberAsync(() =>
		{
			callCount++;
			return ValueTask.CompletedTask;
		});

		await unsubscriber.DisposeAsync();
		await unsubscriber.DisposeAsync();
		await unsubscriber.DisposeAsync();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(callCount, Is.EqualTo(1));
            Assert.That(unsubscriber.IsDisposed, Is.True);
        }
    }

	[Test]
	public void DisposeAsync_WithThrowingFunc_ShouldPropagateException()
	{
		var unsubscriber = new UnsubscriberAsync(() => throw new InvalidOperationException("Test exception"));

		Assert.ThrowsAsync<InvalidOperationException>(async () => await unsubscriber.DisposeAsync());
		Assert.That(unsubscriber.IsDisposed, Is.True);
	}

	[Test]
	public async Task DisposeAsync_WithThrowingFunc_ShouldStillMarkAsDisposed()
	{
		var unsubscriber = new UnsubscriberAsync(() => throw new InvalidOperationException("Test exception"));

		try
		{
			await unsubscriber.DisposeAsync();
		}
		catch (InvalidOperationException)
		{
			
		}

		Assert.That(unsubscriber.IsDisposed, Is.True);
	}

	[Test]
	public async Task DisposeAsync_ShouldSetIsDisposedImmediately()
	{
		var funcExecuted = false;
		var unsubscriber = new UnsubscriberAsync(async () =>
		{
			await Task.Delay(100);
			funcExecuted = true;
		});

		var disposeTask = unsubscriber.DisposeAsync();

		await Task.Delay(10);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(unsubscriber.IsDisposed, Is.True);
            Assert.That(funcExecuted, Is.False);
        }

        await disposeTask;
        
		Assert.That(funcExecuted, Is.True);
	}

	[Test]
	public async Task DisposeAsync_ShouldWorkWithAsyncOperation()
	{
		var value = 0;
		var unsubscriber = new UnsubscriberAsync(async () =>
		{
			await Task.Delay(50);
			value = 42;
		});

		await unsubscriber.DisposeAsync();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(value, Is.EqualTo(42));
            Assert.That(unsubscriber.IsDisposed, Is.True);
        }
    }

	[Test]
	public Task DisposeAsync_AfterException_ShouldRemainIdempotent()
	{
		var callCount = 0;
		var unsubscriber = new UnsubscriberAsync(() =>
		{
			callCount++;
			
			return callCount == 1 
				? throw new InvalidOperationException("First call fails") 
				: ValueTask.CompletedTask;
		});

		Assert.ThrowsAsync<InvalidOperationException>(async () => await unsubscriber.DisposeAsync());
		
		Assert.DoesNotThrowAsync(async () => await unsubscriber.DisposeAsync());
		Assert.DoesNotThrowAsync(async () => await unsubscriber.DisposeAsync());

        using (Assert.EnterMultipleScope())
        {
            Assert.That(callCount, Is.EqualTo(1));
            Assert.That(unsubscriber.IsDisposed, Is.True);
        }

        return Task.CompletedTask;
	}

	[Test]
	public Task DisposeAsync_WithCancellationInFunc_ShouldWorkCorrectly()
	{
		var unsubscriber = new UnsubscriberAsync(async () =>
		{
			using var cts = new CancellationTokenSource(1000);
			await Task.Delay(Timeout.Infinite, cts.Token);
		});

		Assert.DoesNotThrowAsync(async () => await unsubscriber.DisposeAsync());
		Assert.That(unsubscriber.IsDisposed, Is.True);
		
		return Task.CompletedTask;
	}
}