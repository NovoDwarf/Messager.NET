using BenchmarkDotNet.Attributes;
using Messager.NET.Entities;
using Messager.NET.Senders;

namespace Messager.NET.Benchmarks.Benchmarks;

[MemoryDiagnoser]
public class ThroughputBenchmark
{
    private event EventHandler<int>? StandardEvent;

    private readonly List<IDisposable> _subscriptions = [];
    
    private ISender<int> _sender = null!;
    private int _receivedCount;

    [Params(1, 10, 100)]
    public int SubscriberCount { get; set; }

    [Params(1000, 10000, 100000)]
    public int TotalMessages { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        var exchange = new Exchange();

        _sender = exchange.GetSender<int>();
        var receiver = exchange.GetReceiver<int>();

        for (var i = 0; i < SubscriberCount; i++)
        {
            StandardEvent += OnStandardReceived;
            _subscriptions.Add(receiver.Subscribe(OnReceived));
        }

        if (StandardEvents_Throughput() != TotalMessages * SubscriberCount)
            throw new InvalidOperationException("Standard event delivery count mismatch.");

        if (Messager_Throughput() != TotalMessages * SubscriberCount)
            throw new InvalidOperationException("Messager delivery count mismatch.");
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        foreach (var subscription in _subscriptions)
            subscription.Dispose();

        _subscriptions.Clear();
        StandardEvent = null;
    }

    [Benchmark(Baseline = true)]
    public int StandardEvents_Throughput()
    {
        _receivedCount = 0;

        for (var i = 0; i < TotalMessages; i++)
            StandardEvent?.Invoke(this, i);

        return _receivedCount;
    }

    [Benchmark]
    public int Messager_Throughput()
    {
        _receivedCount = 0;

        for (var i = 0; i < TotalMessages; i++)
            _sender.Send(i);

        return _receivedCount;
    }

    private void OnStandardReceived(object? sender, int message) => OnReceived(message);

    private void OnReceived(int message) => _receivedCount++;
}