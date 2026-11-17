using System.Reflection;
using System.Runtime.CompilerServices;
using Messager.NET.Entities;

namespace Messager.NET.Tests.Models.Core;

[TestFixture]
public sealed class WeakActionMulticastTests
{
    [Test]
    public void Matches_RequiresCompleteInvocationListInOrder()
    {
        var first = new Subscriber();
        var second = new Subscriber();
        Action<int> firstHandler = first.Handle;
        Action<int> secondHandler = second.Handle;
        var action = new WeakAction<int>(firstHandler + secondHandler);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(action.Matches(firstHandler + secondHandler), Is.True);
            Assert.That(action.Matches(secondHandler), Is.False);
            Assert.That(action.Matches(secondHandler + firstHandler), Is.False);
            Assert.That(action.Matches(firstHandler + firstHandler), Is.False);
        }
    }

    [Test]
    public void TryInvoke_CollectedTarget_SkipsOnlyCollectedCallback()
    {
        var survivor = new Subscriber();
        var action = CreateWithCollectableTarget(survivor, out var collected);

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(collected.IsAlive, Is.False);
            Assert.That(action.IsAlive, Is.True);
            Assert.That(action.TryInvoke(42), Is.True);
            Assert.That(survivor.Value, Is.EqualTo(42));
        }

        GC.KeepAlive(survivor);
    }

    [Test]
    public void TryInvoke_AllTargetsCollected_ReturnsFalse()
    {
        var action = CreateCollectableMulticast();

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(action.IsAlive, Is.False);
            Assert.That(action.TryInvoke(42), Is.False);
        }
    }

    [Test]
    public void TryInvoke_ClosedStaticDelegate_PreservesBoundArgument()
    {
        var subscriber = new Subscriber();
        var method = typeof(WeakActionMulticastTests).GetMethod(
            nameof(HandleStatic), BindingFlags.Static | BindingFlags.NonPublic)!;
        var handler = method.CreateDelegate<Action<int>>(subscriber);
        var action = new WeakAction<int>(handler);

        Assert.That(action.TryInvoke(42), Is.True);
        Assert.That(subscriber.Value, Is.EqualTo(42));
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakAction<int> CreateWithCollectableTarget(Subscriber survivor, out WeakReference collected)
    {
        var temporary = new Subscriber();
        collected = new WeakReference(temporary);
        Action<int> first = temporary.Handle;
        return new WeakAction<int>(first + survivor.Handle);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakAction<int> CreateCollectableMulticast()
    {
        var first = new Subscriber();
        var second = new Subscriber();
        Action<int> handler = first.Handle;
        return new WeakAction<int>(handler + second.Handle);
    }

    private static void HandleStatic(Subscriber subscriber, int value)
    {
        subscriber.Handle(value);
    }

    private sealed class Subscriber
    {
        internal int Value { get; private set; }

        internal void Handle(int value)
        {
            Value = value;
        }
    }
}
