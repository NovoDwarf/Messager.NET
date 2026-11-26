using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;
using Messager.NET.Models.Brokers;
using Messager.NET.Receivers;
using static Messager.NET.Tests.Helpers.TestHelpers;

namespace Messager.NET.Tests.Brokers.Broker;

[TestFixture]
public sealed partial class Broker_Tests
{
    private const string Key = "key";

    [TestCase(false, false)]
    [TestCase(false, true)]
    [TestCase(true, false)]
    [TestCase(true, true)]
    public async Task Send_RemovesCollectedSubscriptionsAndPreservesLiveOnes(bool asynchronous, bool keyed)
    {
        var broker = CreateBroker(asynchronous, keyed);
        var survivor = new Target();

        Subscribe(broker, survivor, asynchronous, keyed, SubscriptionReference.Strong);
        var collected = CreateWeakSubscriptions(broker, asynchronous, keyed);

        CollectGarbage();
        Assert.That(collected.All(reference => !reference.IsAlive), Is.True);

        await Send(broker, asynchronous, keyed);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(survivor.Calls, Is.EqualTo(1));
            Assert.That(RegistrationCount(broker, keyed), Is.EqualTo(1));
        }

        GC.KeepAlive(survivor);
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task KeyedSend_RemovesKeyWhenLastTargetIsCollected(bool asynchronous)
    {
        object broker = asynchronous ? new AsyncKeyedBroker<string, int>() : new KeyedBroker<string, int>();
        var collected = CreateWeakSubscriptions(broker, asynchronous, true);
        CollectGarbage();
        Assert.That(collected.All(reference => !reference.IsAlive), Is.True);

        await Send(broker, asynchronous, true);

        Assert.That(RegistrationCount(broker, true), Is.Zero);
    }

    [TestCase(false, false)]
    [TestCase(false, true)]
    [TestCase(true, false)]
    [TestCase(true, true)]
    public async Task Cleanup_PreservesSubscriptionsAddedDuringSend(bool asynchronous, bool keyed)
    {
        var broker = CreateBroker(asynchronous, keyed);
        var newcomer = new Target();
        var survivor = new Target();

        survivor.OnHandle = () =>
        {
            survivor.OnHandle = null;
            Subscribe(broker, newcomer, asynchronous, keyed, SubscriptionReference.Strong);
        };

        Subscribe(broker, survivor, asynchronous, keyed, SubscriptionReference.Strong);
        var collected = CreateWeakSubscriptions(broker, asynchronous, keyed);

        CollectGarbage();
        Assert.That(collected.All(reference => !reference.IsAlive), Is.True);

        await Send(broker, asynchronous, keyed);
        Assert.That(RegistrationCount(broker, keyed), Is.EqualTo(2));

        await Send(broker, asynchronous, keyed);
        Assert.That(newcomer.Calls, Is.EqualTo(1));
    }

    private static object CreateBroker(bool asynchronous, bool keyed)
    {
        return (asynchronous, keyed) switch
        {
            (false, false) => new SimpleBroker<int>(),
            (false, true) => new KeyedBroker<string, int>(),
            (true, false) => new AsyncSimpleBroker<int>(),
            _ => new AsyncKeyedBroker<string, int>()
        };
    }


    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference[] CreateWeakSubscriptions(object broker, bool asynchronous, bool keyed)
    {
        var references = new WeakReference[100];
        for (var i = 0; i < references.Length; i++)
        {
            var target = new Target();
            references[i] = new WeakReference(target);
            Subscribe(broker, target, asynchronous, keyed, SubscriptionReference.Weak);
        }

        return references;
    }

    private static void Subscribe(object broker, Target target, bool asynchronous, bool keyed, SubscriptionReference reference)
    {
        if (asynchronous && keyed)
            ((AsyncKeyedBroker<string, int>)broker).Subscribe(Key, target.HandleAsync, reference);
        else if (asynchronous)
            ((AsyncSimpleBroker<int>)broker).Subscribe(target.HandleAsync, reference);
        else if (keyed)
            ((KeyedBroker<string, int>)broker).Subscribe(Key, target.Handle, reference);
        else
            ((SimpleBroker<int>)broker).Subscribe(target.Handle, reference);
    }

    private static async Task Send(object broker, bool asynchronous, bool keyed)
    {
        if (asynchronous && keyed)
            await ((AsyncKeyedBroker<string, int>)broker).SendAsync(Key, 1);
        else if (asynchronous)
            await ((AsyncSimpleBroker<int>)broker).SendAsync(1);
        else if (keyed)
            ((KeyedBroker<string, int>)broker).Send(Key, 1);
        else
            ((SimpleBroker<int>)broker).Send(1);
    }

    private static int RegistrationCount(object broker, bool keyed)
    {
        var field = broker.GetType().GetField("_handlers", BindingFlags.Instance | BindingFlags.NonPublic)!;
        var handlers = field.GetValue(broker)!;

        if (!keyed)
            return ((Array)handlers).Length;

        var subscriptions = ((IDictionary)handlers)[Key] as Array;
        return subscriptions?.Length ?? 0;
    }

    private sealed class Target
    {
        public int Calls { get; private set; }
        public Action? OnHandle { get; set; }

        public void Handle(int value)
        {
            Calls += value;
            OnHandle?.Invoke();
        }

        public ValueTask HandleAsync(int value)
        {
            Handle(value);
            return ValueTask.CompletedTask;
        }
    }
}
