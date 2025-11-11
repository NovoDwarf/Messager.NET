namespace Messager.NET.Entity.Resources;

public sealed class AsyncUnsubscriber : IAsyncDisposable
{
	private readonly Func<ValueTask> _unsubscribe;
	
	public AsyncUnsubscriber(Func<ValueTask> unsubscribe)
	{
		_unsubscribe = unsubscribe;
	}
	
	private bool _disposed;

	public async ValueTask DisposeAsync()
	{
		if (_disposed)
			return;

		_disposed = true;
		
		await _unsubscribe();
	}
}