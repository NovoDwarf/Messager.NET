namespace Messager.NET.Entity.Resources;

public sealed class SmartDisposable : IDisposable
{
	private readonly IDisposable _disposable;

	public SmartDisposable(IDisposable disposable)
	{
		_disposable = disposable;
	}

	private bool _disposed;
	
	public void Dispose()
	{
		if (_disposed)
			return;
		
		_disposable.Dispose();
		_disposed = true;
	}
}