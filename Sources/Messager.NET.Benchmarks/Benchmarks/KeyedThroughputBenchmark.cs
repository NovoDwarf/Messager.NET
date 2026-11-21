using BenchmarkDotNet.Attributes;
using Messager.NET.Entities;
using Messager.NET.Senders;

namespace Messager.NET.Benchmarks.Benchmarks;

[MemoryDiagnoser]
public class KeyedThroughputBenchmark
{
	private ISender<int, int> _sender = null!;
	private IDisposable _subscription = null!;

	private int _receivedCount;

	[Params(1000, 10000, 100000)]
	public int TotalMessages { get; set; }

	[GlobalSetup]
	public void Setup()
	{
		var exchange = new Exchange();

		_sender = exchange.GetKeyedSender<int, int>();
		_subscription = exchange.GetKeyedReceiver<int, int>().Subscribe(42, OnReceived);
	}

	[Benchmark]
	public int KeyedSend()
	{
		_receivedCount = 0;

		for (var i = 0; i < TotalMessages; i++)
			_sender.Send(42, i);

		return _receivedCount;
	}

	[GlobalCleanup]
	public void Cleanup() => _subscription.Dispose();

	private void OnReceived(int message) => _receivedCount++;
}