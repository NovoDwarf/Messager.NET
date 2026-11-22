using BenchmarkDotNet.Attributes;
using Messager.NET.Entities;
using Messager.NET.Receivers;
using Messager.NET.Senders;

namespace Messager.NET.Benchmarks.Benchmarks;

[MemoryDiagnoser]
public class WeakSubscriptionBenchmark
{
	private ISender<int> _strongSender = null!;
	private ISender<int> _weakSender = null!;

	private IDisposable _strongSubscription = null!;
	private IDisposable _weakSubscription = null!;

	private Receiver _strongTarget = null!;
	private Receiver _weakTarget = null!;

	[Params(1000, 10000)]
	public int TotalMessages { get; set; }

	[GlobalSetup]
	public void Setup()
	{
		var strongExchange = new Exchange();
		var weakExchange = new Exchange();

		_strongTarget = new Receiver();
		_weakTarget = new Receiver();

		_strongSender = strongExchange.GetSender<int>();
		_weakSender = weakExchange.GetSender<int>();

		_strongSubscription = strongExchange.GetReceiver<int>().Subscribe(_strongTarget.Handle, SubscriptionReference.Strong);
		_weakSubscription = weakExchange.GetReceiver<int>().Subscribe(_weakTarget.Handle, SubscriptionReference.Weak);
	}

	[Benchmark(Baseline = true)]
	public int StrongSubscription()
	{
		for (var i = 0; i < TotalMessages; i++)
			_strongSender.Send(i);

		return _strongTarget.Count;
	}

	[Benchmark]
	public int WeakSubscription()
	{
		for (var i = 0; i < TotalMessages; i++)
			_weakSender.Send(i);

		return _weakTarget.Count;
	}

	[GlobalCleanup]
	public void Cleanup()
	{
		_strongSubscription.Dispose();
		_weakSubscription.Dispose();

		GC.KeepAlive(_strongTarget);
		GC.KeepAlive(_weakTarget);
	}

	private sealed class Receiver
	{
		public int Count { get; private set; }

		public void Handle(int message)
		{
			Count++;
		}
	}
}