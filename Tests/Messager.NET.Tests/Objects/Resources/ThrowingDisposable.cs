namespace Messager.NET.Tests.Objects.Resources;

internal class ThrowingDisposable : IDisposable
{
	private readonly string _message;

	public bool IsDisposed { get; private set; }

	public ThrowingDisposable(string message = "Test exception")
	{
		_message = message;
	}

	public void Dispose()
	{
		IsDisposed = true;
		throw new InvalidOperationException(_message);
	}
}