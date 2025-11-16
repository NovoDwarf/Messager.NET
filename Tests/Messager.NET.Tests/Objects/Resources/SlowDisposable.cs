namespace Messager.NET.Tests.Objects.Resources;

internal class SlowDisposable : IDisposable
{
	private readonly TimeSpan _delay;

	public SlowDisposable(TimeSpan delay)
	{
		_delay = delay;
	}

	public void Dispose()
	{
		Thread.Sleep(_delay);
	}
}