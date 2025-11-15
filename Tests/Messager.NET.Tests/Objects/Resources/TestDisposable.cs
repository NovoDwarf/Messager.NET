namespace Messager.NET.Tests.Objects.Resources;

// Test helper classes
internal class TestDisposable : IDisposable
{
	public bool IsDisposed { get; private set; }
	public int DisposeCount { get; private set; }

	public void Dispose()
	{
		IsDisposed = true;
		DisposeCount++;
	}
}