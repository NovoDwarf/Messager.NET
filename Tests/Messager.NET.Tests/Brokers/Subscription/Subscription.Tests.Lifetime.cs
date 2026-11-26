using Messager.NET.Entities;
using Messager.NET.Receivers;
using static Messager.NET.Tests.Helpers.TestHelpers;

namespace Messager.NET.Tests.Brokers.Subscription;

public sealed partial class Subscription_Tests
{
    [TestCase(false, false)]
    [TestCase(false, true)]
    [TestCase(true, false)]
    [TestCase(true, true)]
    public async Task WeakSubscription_StaticCallbackRemainsAlive(bool asynchronous, bool keyed)
    {
        var exchange = new Exchange();
        _staticCalls = 0;
        object handle;

        if (asynchronous)
        {
            handle = keyed
                ? exchange.GetAsyncKeyedReceiver<string, int>().Subscribe(Key, HandleStaticAsync, SubscriptionReference.Weak)
                : exchange.GetAsyncReceiver<int>().Subscribe(HandleStaticAsync, SubscriptionReference.Weak);
        }
        else
        {
            handle = keyed
                ? exchange.GetKeyedReceiver<string, int>().Subscribe(Key, HandleStatic, SubscriptionReference.Weak)
                : exchange.GetReceiver<int>().Subscribe(HandleStatic, SubscriptionReference.Weak);
        }

        CollectGarbage();
        await SendAsync(exchange, asynchronous, keyed);
        Assert.That(_staticCalls, Is.EqualTo(1));

        await DisposeAsync(handle);
        await SendAsync(exchange, asynchronous, keyed);
        Assert.That(_staticCalls, Is.EqualTo(1));
    }

    [TestCase(false, false, SubscriptionReference.Strong)]
    [TestCase(false, true, SubscriptionReference.Strong)]
    [TestCase(true, false, SubscriptionReference.Strong)]
    [TestCase(true, true, SubscriptionReference.Strong)]
    [TestCase(false, false, SubscriptionReference.Weak)]
    [TestCase(false, true, SubscriptionReference.Weak)]
    [TestCase(true, false, SubscriptionReference.Weak)]
    [TestCase(true, true, SubscriptionReference.Weak)]
    public async Task ReferenceMode_ControlsTargetLifetimeWhileHandleIsRetained(
        bool asynchronous, bool keyed, SubscriptionReference reference)
    {
        var exchange = new Exchange();
        var counter = new Counter();
        var (handle, target) = CreateSubscription(exchange, counter, asynchronous, keyed, reference);

        CollectGarbage();
        Assert.That(target.IsAlive, Is.EqualTo(reference == SubscriptionReference.Strong));

        await SendAsync(exchange, asynchronous, keyed);
        Assert.That(counter.Calls, Is.EqualTo(reference == SubscriptionReference.Strong ? 1 : 0));

        await DisposeAsync(handle);
        CollectGarbage();

        Assert.That(target.IsAlive, Is.False);
        await SendAsync(exchange, asynchronous, keyed);
        Assert.That(counter.Calls, Is.EqualTo(reference == SubscriptionReference.Strong ? 1 : 0));

        GC.KeepAlive(handle);
        GC.KeepAlive(exchange);
    }

    [TestCase(false, false)]
    [TestCase(false, true)]
    [TestCase(true, false)]
    [TestCase(true, true)]
    public async Task StrongSubscription_BrokerRetainsTargetWhenHandleIsDiscarded(bool asynchronous, bool keyed)
    {
        var exchange = new Exchange();
        var counter = new Counter();
        var target = CreateSubscriptionWithoutHandle(exchange, counter, asynchronous, keyed);

        CollectGarbage();
        Assert.That(target.IsAlive, Is.True);

        await SendAsync(exchange, asynchronous, keyed);
        Assert.That(counter.Calls, Is.EqualTo(1));

        GC.KeepAlive(exchange);
    }

    [TestCase(false, false)]
    [TestCase(false, true)]
    [TestCase(true, false)]
    [TestCase(true, true)]
    public async Task WeakMulticast_CollectedCallbackDoesNotRemoveSurvivingCallback(bool asynchronous, bool keyed)
    {
        var exchange = new Exchange();
        var counter = new Counter();
        var survivor = new Subscriber(counter);
        var (handle, target) = CreateWeakMulticast(exchange, survivor, asynchronous, keyed);

        CollectGarbage();
        Assert.That(target.IsAlive, Is.False);
        await SendAsync(exchange, asynchronous, keyed);
        Assert.That(counter.Calls, Is.EqualTo(1));

        await DisposeAsync(handle);
        await SendAsync(exchange, asynchronous, keyed);
        Assert.That(counter.Calls, Is.EqualTo(1));
        GC.KeepAlive(survivor);
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task Unsubscribe_WeakAsyncMulticast_MatchesCompleteInvocationList(bool keyed)
    {
        var exchange = new Exchange();
        var counter = new Counter();
        var first = new Subscriber(counter);
        var second = new Subscriber(counter);
        var handler = first.HandleAsync;

        handler += second.HandleAsync;

        var receiver = exchange.GetAsyncReceiver<int>();
        var keyedReceiver = exchange.GetAsyncKeyedReceiver<string, int>();

        await using var subscription = keyed
            ? keyedReceiver.Subscribe(Key, handler, SubscriptionReference.Weak)
            : receiver.Subscribe(handler, SubscriptionReference.Weak);

        if (keyed)
            keyedReceiver.Unsubscribe(Key, second.HandleAsync);
        else
            receiver.Unsubscribe(second.HandleAsync);

        await SendAsync(exchange, true, keyed);
        Assert.That(counter.Calls, Is.EqualTo(2));

        if (keyed)
            keyedReceiver.Unsubscribe(Key, handler);
        else
            receiver.Unsubscribe(handler);

        await SendAsync(exchange, true, keyed);
        Assert.That(counter.Calls, Is.EqualTo(2));
        GC.KeepAlive(handler);
    }
}
