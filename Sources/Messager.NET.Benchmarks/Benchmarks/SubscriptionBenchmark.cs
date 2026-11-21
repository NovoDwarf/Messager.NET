using BenchmarkDotNet.Attributes;
using Messager.NET.Entities;
using Messager.NET.Receivers;

namespace Messager.NET.Benchmarks.Benchmarks;

[MemoryDiagnoser]
public class SubscriptionBenchmark
{
	private IReceiver<int> _receiver = null!;
	private Action<int> _handler = null!;

	[Params(0, 10, 100, 1000)]
	public int ExistingSubscribers { get; set; }

	[GlobalSetup]
	public void Setup()
	{
		var exchange = new Exchange();
       
		_receiver = exchange.GetReceiver<int>();
		_handler = OnReceived;

		for (var i = 0; i < ExistingSubscribers; i++) 
			_receiver.Subscribe(_handler);
	}

	[Benchmark]
	public void SubscribeAndDispose()
	{
		using var subscription = _receiver.Subscribe(_handler);
	}

	private void OnReceived(int message) { }
}