using System.Runtime.CompilerServices;
using Messager.NET.Entities;
using Messager.NET.Receivers;

namespace Messager.NET.Tests.Models.Brokers;

[TestFixture]
public sealed class SubscriptionReferenceTests
{
    private static int _staticCalls;

    [TestCase(false)]
    [TestCase(true)]
    public async Task Unsubscribe_StrongAsyncHandler_ReleasesTargetWhileHandleIsRetained(bool keyed)
    {
        var exchange = new Exchange();
        var (handle, target) = CreateSubscription(exchange, new Counter(), true, keyed, SubscriptionReference.Strong);

        Unsubscribe(exchange, target, keyed);
        Collect();

        Assert.That(target.IsAlive, Is.False);
        await DisposeAsync(handle);
        GC.KeepAlive(handle);
        GC.KeepAlive(exchange);
    }

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
                ? exchange.GetAsyncKeyedReceiver<string, int>().Subscribe("key", HandleStaticAsync, SubscriptionReference.Weak)
                : exchange.GetAsyncReceiver<int>().Subscribe(HandleStaticAsync, SubscriptionReference.Weak);
        }
        else
        {
            handle = keyed
                ? exchange.GetKeyedReceiver<string, int>().Subscribe("key", HandleStatic, SubscriptionReference.Weak)
                : exchange.GetReceiver<int>().Subscribe(HandleStatic, SubscriptionReference.Weak);
        }

        Collect();
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

        Collect();
        Assert.That(target.IsAlive, Is.EqualTo(reference == SubscriptionReference.Strong));

        await SendAsync(exchange, asynchronous, keyed);
        Assert.That(counter.Calls, Is.EqualTo(reference == SubscriptionReference.Strong ? 1 : 0));

        await DisposeAsync(handle);
        Collect();

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

        Collect();
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

        Collect();
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
            ? keyedReceiver.Subscribe("key", handler, SubscriptionReference.Weak)
            : receiver.Subscribe(handler, SubscriptionReference.Weak);

        if (keyed)
        {
            keyedReceiver.Unsubscribe("key", second.HandleAsync);
        }
        else
        {
            receiver.Unsubscribe(second.HandleAsync);
        }

        await SendAsync(exchange, true, keyed);
        Assert.That(counter.Calls, Is.EqualTo(2));

        if (keyed)
        {
            keyedReceiver.Unsubscribe("key", handler);
        }
        else
        {
            receiver.Unsubscribe(handler);
        }

        await SendAsync(exchange, true, keyed);
        Assert.That(counter.Calls, Is.EqualTo(2));
        GC.KeepAlive(handler);
    }

    [TestCase(false, false)]
    [TestCase(false, true)]
    [TestCase(true, false)]
    [TestCase(true, true)]
    public void Subscribe_InvalidReferenceMode_ThrowsBeforeRegistration(bool asynchronous, bool keyed)
    {
        var exchange = new Exchange();

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            CreateSubscription(exchange, new Counter(), asynchronous, keyed, (SubscriptionReference)99));
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static (object Handle, WeakReference Target) CreateSubscription(Exchange exchange, Counter counter, bool asynchronous, bool keyed, SubscriptionReference reference)
    {
        var subscriber = new Subscriber(counter);
        object handle;

        if (asynchronous)
        {
            handle = keyed
                ? exchange.GetAsyncKeyedReceiver<string, int>().Subscribe("key", subscriber.HandleAsync, reference)
                : exchange.GetAsyncReceiver<int>().Subscribe(subscriber.HandleAsync, reference);
        }
        else
        {
            handle = keyed
                ? exchange.GetKeyedReceiver<string, int>().Subscribe("key", subscriber.Handle, reference)
                : exchange.GetReceiver<int>().Subscribe(subscriber.Handle, reference);
        }

        return (handle, new WeakReference(subscriber));
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference CreateSubscriptionWithoutHandle(Exchange exchange, Counter counter, bool asynchronous, bool keyed)
    {
        var (_, target) = CreateSubscription(exchange, counter, asynchronous, keyed, SubscriptionReference.Strong);
        return target;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static (object Handle, WeakReference Target) CreateWeakMulticast(
        Exchange exchange, Subscriber survivor, bool asynchronous, bool keyed)
    {
        var temporary = new Subscriber(new Counter());
        object handle;

        if (asynchronous)
        {
            var handler = temporary.HandleAsync;
          
            handler += survivor.HandleAsync;
           
            handle = keyed
                ? exchange.GetAsyncKeyedReceiver<string, int>().Subscribe("key", handler, SubscriptionReference.Weak)
                : exchange.GetAsyncReceiver<int>().Subscribe(handler, SubscriptionReference.Weak);
        }
        else
        {
            Action<int> handler = temporary.Handle;
            handler += survivor.Handle;
            handle = keyed
                ? exchange.GetKeyedReceiver<string, int>().Subscribe("key", handler, SubscriptionReference.Weak)
                : exchange.GetReceiver<int>().Subscribe(handler, SubscriptionReference.Weak);
        }

        return (handle, new WeakReference(temporary));
    }

    private static async ValueTask SendAsync(Exchange exchange, bool asynchronous, bool keyed)
    {
        if (asynchronous)
        {
            if (keyed)
            {
                await exchange.GetAsyncKeyedSender<string, int>().SendAsync("key", 1);
            }
            else
            {
                await exchange.GetAsyncSender<int>().SendAsync(1);
            }

            return;
        }

        if (keyed)
        {
            exchange.GetKeyedSender<string, int>().Send("key", 1);
        }
        else
        {
            exchange.GetSender<int>().Send(1);
        }
    }

    private static async ValueTask DisposeAsync(object handle)
    {
        if (handle is IAsyncDisposable asynchronous)
        {
            await asynchronous.DisposeAsync();
        }
        else
        {
            ((IDisposable)handle).Dispose();
        }
    }

    private static void Collect()
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void Unsubscribe(Exchange exchange, WeakReference target, bool keyed)
    {
        var subscriber = (Subscriber)target.Target!;

        if (keyed)
        {
            exchange.GetAsyncKeyedReceiver<string, int>().Unsubscribe("key", subscriber.HandleAsync);
        }
        else
        {
            exchange.GetAsyncReceiver<int>().Unsubscribe(subscriber.HandleAsync);
        }
    }

    private static void HandleStatic(int value)
    {
        _staticCalls++;
    }

    private static ValueTask HandleStaticAsync(int value)
    {
        _staticCalls++;
        return ValueTask.CompletedTask;
    }

    private sealed class Counter
    {
        internal int Calls { get; set; }
    }

    private sealed class Subscriber(Counter counter)
    {
        internal void Handle(int value)
        {
            counter.Calls++;
        }

        internal ValueTask HandleAsync(int value)
        {
            counter.Calls++;
            return ValueTask.CompletedTask;
        }
    }
}

