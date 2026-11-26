using System.Runtime.CompilerServices;
using Messager.NET.Entities;
using Messager.NET.Receivers;

namespace Messager.NET.Tests.Brokers.Subscription;

public sealed partial class Subscription_Tests
{
    private const string Key = "key";
    private static int _staticCalls;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static (object Handle, WeakReference Target) CreateSubscription(Exchange exchange, Counter counter, bool asynchronous, bool keyed, SubscriptionReference reference)
    {
        var subscriber = new Subscriber(counter);
        object handle;

        if (asynchronous)
        {
            handle = keyed
                ? exchange.GetAsyncKeyedReceiver<string, int>().Subscribe(Key, subscriber.HandleAsync, reference)
                : exchange.GetAsyncReceiver<int>().Subscribe(subscriber.HandleAsync, reference);
        }
        else
        {
            handle = keyed
                ? exchange.GetKeyedReceiver<string, int>().Subscribe(Key, subscriber.Handle, reference)
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
    private static (object Handle, WeakReference Target) CreateWeakMulticast(Exchange exchange, Subscriber survivor, bool asynchronous, bool keyed)
    {
        var temporary = new Subscriber(new Counter());
        object handle;

        if (asynchronous)
        {
            var handler = temporary.HandleAsync;

            handler += survivor.HandleAsync;

            handle = keyed
                ? exchange.GetAsyncKeyedReceiver<string, int>().Subscribe(Key, handler, SubscriptionReference.Weak)
                : exchange.GetAsyncReceiver<int>().Subscribe(handler, SubscriptionReference.Weak);
        }
        else
        {
            var handler = temporary.Handle;

            handler += survivor.Handle;
            handle = keyed
                ? exchange.GetKeyedReceiver<string, int>().Subscribe(Key, handler, SubscriptionReference.Weak)
                : exchange.GetReceiver<int>().Subscribe(handler, SubscriptionReference.Weak);
        }

        return (handle, new WeakReference(temporary));
    }

    private static async ValueTask SendAsync(Exchange exchange, bool asynchronous, bool keyed)
    {
        if (asynchronous)
        {
            if (keyed)
                await exchange.GetAsyncKeyedSender<string, int>().SendAsync(Key, 1);
            else
                await exchange.GetAsyncSender<int>().SendAsync(1);

            return;
        }

        if (keyed)
            exchange.GetKeyedSender<string, int>().Send(Key, 1);
        else
            exchange.GetSender<int>().Send(1);

    }

    private static async ValueTask DisposeAsync(object handle)
    {
        if (handle is IAsyncDisposable asynchronous)
            await asynchronous.DisposeAsync();
        else
            ((IDisposable)handle).Dispose();

    }


    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void Unsubscribe(Exchange exchange, WeakReference target, bool keyed)
    {
        var subscriber = (Subscriber)target.Target!;

        if (keyed)
            exchange.GetAsyncKeyedReceiver<string, int>().Unsubscribe(Key, subscriber.HandleAsync);
        else
            exchange.GetAsyncReceiver<int>().Unsubscribe(subscriber.HandleAsync);
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
