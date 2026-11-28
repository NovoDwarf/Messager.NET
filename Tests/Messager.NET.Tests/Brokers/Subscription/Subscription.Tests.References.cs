using Messager.NET.Entities;
using Messager.NET.Enums;
using Messager.NET.Receivers;
using static Messager.NET.Tests.Helpers.TestHelpers;

namespace Messager.NET.Tests.Brokers.Subscription;

public sealed partial class Subscription_Tests
{
    [TestCase(false)]
    [TestCase(true)]
    public async Task Unsubscribe_StrongAsyncHandler_ReleasesTargetWhileHandleIsRetained(bool keyed)
    {
        var exchange = new Exchange();
        var (handle, target) = CreateSubscription(exchange, new Counter(), true, keyed, SubscriptionReference.Strong);

        Unsubscribe(exchange, target, keyed);
        CollectGarbage();

        Assert.That(target.IsAlive, Is.False);

        await DisposeAsync(handle);

        GC.KeepAlive(handle);
        GC.KeepAlive(exchange);
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task RepeatedDisposeAfterUnsubscribe_PreservesDuplicateSubscription(bool keyed)
    {
        var exchange = new Exchange();
        var counter = new Counter();
        var subscriber = new Subscriber(counter);
        var receiver = exchange.GetAsyncReceiver<int>();
        var keyedReceiver = exchange.GetAsyncKeyedReceiver<string, int>();

        var first = keyed
            ? keyedReceiver.Subscribe(Key, subscriber.HandleAsync)
            : receiver.Subscribe(subscriber.HandleAsync);

        var second = keyed
            ? keyedReceiver.Subscribe(Key, subscriber.HandleAsync)
            : receiver.Subscribe(subscriber.HandleAsync);

        if (keyed)
            keyedReceiver.Unsubscribe(Key, subscriber.HandleAsync);
        else
            receiver.Unsubscribe(subscriber.HandleAsync);

        await Task.WhenAll(
            Task.Run(async () => await first.DisposeAsync()),
            Task.Run(async () => await first.DisposeAsync()));

        await SendAsync(exchange, true, keyed);

        Assert.That(counter.Calls, Is.EqualTo(1));

        await second.DisposeAsync();
        await SendAsync(exchange, true, keyed);

        Assert.That(counter.Calls, Is.EqualTo(1));
        GC.KeepAlive(subscriber);
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task Unsubscribe_DuringSend_AllowsCurrentInvocationToComplete(bool keyed)
    {
        var exchange = new Exchange();
        var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var resume = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var calls = 0;

        var receiver = exchange.GetAsyncReceiver<int>();
        var keyedReceiver = exchange.GetAsyncKeyedReceiver<string, int>();

        var handle = keyed
            ? keyedReceiver.Subscribe(Key, Handle)
            : receiver.Subscribe(Handle);

        var send = SendAsync(exchange, true, keyed).AsTask();
        await started.Task.WaitAsync(DefaultTimeout);

        if (keyed)
            keyedReceiver.Unsubscribe(Key, Handle);
        else
            receiver.Unsubscribe(Handle);
        resume.SetResult();

        await send.WaitAsync(DefaultTimeout);
        await SendAsync(exchange, true, keyed);

        Assert.That(calls, Is.EqualTo(1));
        await handle.DisposeAsync();

        return;

        async ValueTask Handle(int value)
        {
            started.TrySetResult();
            await resume.Task;
            calls++;
        }
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task ConcurrentUnsubscribeAndDispose_LeaveConsistentSubscriptions(bool keyed)
    {
        for (var i = 0; i < 50; i++)
        {
            var exchange = new Exchange();
            var counter = new Counter();
            var subscriber = new Subscriber(counter);
            var receiver = exchange.GetAsyncReceiver<int>();
            var keyedReceiver = exchange.GetAsyncKeyedReceiver<string, int>();

            var first = keyed
                ? keyedReceiver.Subscribe(Key, subscriber.HandleAsync)
                : receiver.Subscribe(subscriber.HandleAsync);

            var second = keyed
                ? keyedReceiver.Subscribe(Key, subscriber.HandleAsync)
                : receiver.Subscribe(subscriber.HandleAsync);

            await Task.WhenAll(
                Task.Run(UnsubscribeFrom),
                Task.Run(async () => await first.DisposeAsync()));

            await SendAsync(exchange, true, keyed);

            Assert.That(counter.Calls, Is.InRange(0, 1));
            var calls = counter.Calls;

            await second.DisposeAsync();
            await SendAsync(exchange, true, keyed);

            Assert.That(counter.Calls, Is.EqualTo(calls));

            continue;

            void UnsubscribeFrom()
            {
                if (keyed)
                    keyedReceiver.Unsubscribe(Key, subscriber.HandleAsync);
                else
                    receiver.Unsubscribe(subscriber.HandleAsync);
            }
        }
    }

    [TestCase(false, false)]
    [TestCase(false, true)]
    [TestCase(true, false)]
    [TestCase(true, true)]
    public void Subscribe_InvalidReferenceMode_ThrowsBeforeRegistration(bool asynchronous, bool keyed)
    {
        var exchange = new Exchange();

        Assert.Throws<ArgumentOutOfRangeException>(() => CreateSubscription(exchange, new Counter(), asynchronous, keyed, (SubscriptionReference)99));
    }
}
