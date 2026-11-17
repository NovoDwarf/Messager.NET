using BenchmarkDotNet.Attributes;
using Messager.NET.Entities;
using Messager.NET.Senders;

namespace Messager.NET.Benchmarks.Benchmarks;

[MemoryDiagnoser]
public class ThroughputBenchmark
{
    private event EventHandler<int>? StandardEvent;
    
    private ISender<int> _messagerSender = null!;
    private IDisposable _subscription = null!;
    private int _receivedCount;

    [Params(1000, 10000, 100000)]
    public int TotalMessages { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        var exchange = new Exchange();
        
        _messagerSender = exchange.GetSender<int>();
        _subscription = exchange.GetReceiver<int>().Subscribe(OnReceived);
       
        StandardEvent = OnStandardReceived;

        // Check repeated calls too, so an accumulating event subscription cannot bias measurements.
        for (var i = 0; i < 2; i++)
        {
            if (StandardEvents_Throughput() != TotalMessages || Messager_Throughput() != TotalMessages)
            {
                throw new InvalidOperationException("Each message must reach exactly one handler.");
            }
        }
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _subscription.Dispose();
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
            _messagerSender.Send(i);

        return _receivedCount;
    }

    private void OnStandardReceived(object? sender, int message)
    {
        OnReceived(message);
    }

    private void OnReceived(int message)
    {
        _receivedCount++;
    }
}
