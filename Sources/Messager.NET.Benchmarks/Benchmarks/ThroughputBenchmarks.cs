using Messager.NET.Interfaces.Receivers;
using Messager.NET.Interfaces.Senders;

namespace Messager.NET.Benchmarks.Benchmarks;

using BenchmarkDotNet.Attributes;

[MemoryDiagnoser]
[SimpleJob(BenchmarkDotNet.Engines.RunStrategy.ColdStart)]
public class ThroughputBenchmark : IDisposable
{
	public ThroughputBenchmark(ISender<int> messagerSender, IReceiver<int> messagerReceiver)
	{
		_messagerSender = messagerSender;
		_disposable = messagerReceiver.Subscribe(OnReceived);
	}
	
	private ISender<int> _messagerSender;
	private IDisposable _disposable;

	private EventHandler<int> _standardEvent;

	private int _messageCount;
	private int _receivedCount;

	[Params(1000, 10000, 100000)] public int TotalMessages;
	
	[GlobalSetup]
	public void Setup()
	{
		_standardEvent = null;

		_messageCount = TotalMessages;
		_receivedCount = 0;
	}

	[GlobalCleanup]
	public void Dispose()
	{
		_disposable.Dispose();
	}

	[Benchmark(Baseline = true)]
	public void StandardEvents_Throughput()
	{
		_receivedCount = 0;
		_standardEvent += (s, e) => { _receivedCount++; };

		for (var i = 0; i < _messageCount; i++)
		{
			_standardEvent?.Invoke(this, i);
		}
	}

	[Benchmark]
	public void Messager_Throughput()
	{
		_receivedCount = 0;

		for (var i = 0; i < _messageCount; i++)
		{
			_messagerSender.Send(i);
		}
	}

	private void OnReceived(int message)
	{
		_receivedCount++;
	}
}