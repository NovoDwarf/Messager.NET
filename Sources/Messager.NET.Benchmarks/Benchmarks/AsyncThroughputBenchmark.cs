using BenchmarkDotNet.Attributes;
using Messager.NET.Entities;
using Messager.NET.Receivers;
using Messager.NET.Senders;

namespace Messager.NET.Benchmarks.Benchmarks;

[MemoryDiagnoser]
public class AsyncThroughputBenchmark
{
	private IAsyncSender<int> _sender = null!;
	private IAsyncDisposable _subscription = null!;

	private int _receivedCount;

	[Params(1000, 10000, 100000)]
	public int TotalMessages { get; set; }

	[GlobalSetup]
	public void Setup()
	{
		var exchange = new Exchange();

		_sender = exchange.GetAsyncSender<int>();
		_subscription = exchange.GetAsyncReceiver<int>().Subscribe(OnReceivedAsync);
	}

	[Benchmark]
	public async Task<int> AsyncSend()
	{
		_receivedCount = 0;

		for (var i = 0; i < TotalMessages; i++) 
			await _sender.SendAsync(i).ConfigureAwait(false);

		return _receivedCount;
	}

	[GlobalCleanup]
	public async Task Cleanup() => await _subscription.DisposeAsync();

	private ValueTask OnReceivedAsync(int message)
	{
		_receivedCount++;
		return ValueTask.CompletedTask;
	}
}