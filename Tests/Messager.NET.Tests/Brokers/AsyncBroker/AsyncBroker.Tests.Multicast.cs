using Messager.NET.Entities;

namespace Messager.NET.Tests.Brokers.AsyncBroker;

[TestFixture]
public sealed partial class AsyncBroker_Tests
{
    private const string Key = "key";

    [TestCase(false)]
    [TestCase(true)]
    public async Task DisposeAsync_AfterExplicitUnsubscribe_DoesNotRemoveReplacement(bool keyed)
    {
        var exchange = new Exchange();
        var calls = 0;

        Func<int, ValueTask> handler = _ => { calls++; return ValueTask.CompletedTask; };

        await using var original = keyed
            ? exchange.GetAsyncKeyedReceiver<string, int>().Subscribe(Key, handler)
            : exchange.GetAsyncReceiver<int>().Subscribe(handler);

        if (keyed)
            exchange.GetAsyncKeyedReceiver<string, int>().Unsubscribe(Key, handler);
        else
            exchange.GetAsyncReceiver<int>().Unsubscribe(handler);

        await using var replacement = keyed
            ? exchange.GetAsyncKeyedReceiver<string, int>().Subscribe(Key, handler)
            : exchange.GetAsyncReceiver<int>().Subscribe(handler);

        // ReSharper disable once DisposeOnUsingVariable
        await original.DisposeAsync();

        if (keyed)
            await exchange.GetAsyncKeyedSender<string, int>().SendAsync(Key, 1);
        else
            await exchange.GetAsyncSender<int>().SendAsync(1);


        Assert.That(calls, Is.EqualTo(1));
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task SendAsync_MulticastHandler_AwaitsEachCallbackInOrder(bool keyed)
    {
        var exchange = new Exchange();
        var entered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var calls = new List<int>();

        Func<int, ValueTask> first = async value => { entered.SetResult(); await release.Task; calls.Add(value); };
        Func<int, ValueTask> second = value => { calls.Add(value + 1); return ValueTask.CompletedTask; };

        await using var subscription = keyed
            ? exchange.GetAsyncKeyedReceiver<string, int>().Subscribe(Key, first + second)
            : exchange.GetAsyncReceiver<int>().Subscribe(first + second);

        var send = keyed
            ? exchange.GetAsyncKeyedSender<string, int>().SendAsync(Key, 10).AsTask()
            : exchange.GetAsyncSender<int>().SendAsync(10).AsTask();

        try
        {
            await entered.Task.WaitAsync(TimeSpan.FromSeconds(5));

            using (Assert.EnterMultipleScope())
            {
                Assert.That(send.IsCompleted, Is.False);
                Assert.That(calls, Is.Empty);
            }
        }
        finally
        {
            release.TrySetResult();
            await send.WaitAsync(TimeSpan.FromSeconds(5));
        }

        Assert.That(calls, Is.EqualTo([10, 11]));

        // ReSharper disable once DisposeOnUsingVariable
        await subscription.DisposeAsync();

        if (keyed)
            await exchange.GetAsyncKeyedSender<string, int>().SendAsync(Key, 20);
        else
            await exchange.GetAsyncSender<int>().SendAsync(20);


        Assert.That(calls, Has.Count.EqualTo(2));
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task SendAsync_ThrowingMulticastHandler_ContinuesWithNextSubscription(bool keyed)
    {
        var exchange = new Exchange();
        var calls = new List<int>();

        Func<int, ValueTask> throwing = async _ => { await Task.Yield(); throw new InvalidOperationException(); };
        Func<int, ValueTask> skipped = _ => { calls.Add(1); return ValueTask.CompletedTask; };
        Func<int, ValueTask> next = _ => { calls.Add(2); return ValueTask.CompletedTask; };

        await using var first = keyed
            ? exchange.GetAsyncKeyedReceiver<string, int>().Subscribe(Key, throwing + skipped)
            : exchange.GetAsyncReceiver<int>().Subscribe(throwing + skipped);

        await using var second = keyed
            ? exchange.GetAsyncKeyedReceiver<string, int>().Subscribe(Key, next)
            : exchange.GetAsyncReceiver<int>().Subscribe(next);

        if (keyed)
            await exchange.GetAsyncKeyedSender<string, int>().SendAsync(Key, 1);
        else
            await exchange.GetAsyncSender<int>().SendAsync(1);

        Assert.That(calls, Is.EqualTo([2]));
    }
}
