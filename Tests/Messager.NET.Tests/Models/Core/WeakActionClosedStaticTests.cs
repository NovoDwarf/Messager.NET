using System.Reflection;
using System.Runtime.CompilerServices;
using Messager.NET.Entities;

namespace Messager.NET.Tests.Models.Core;

[TestFixture]
public sealed class WeakActionClosedStaticTests
{
    [TestCase(false)]
    [TestCase(true)]
    public async Task ClosedStatic_UsesDeclaredTargetTypeAndDoesNotRetainTarget(bool asynchronous)
    {
        var (action, target, counter) = CreateAction(asynchronous);
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        Assert.That(target.IsAlive, Is.False);
        if (action is WeakAction<int> synchronous)
        {
            Assert.That(synchronous.TryInvoke(42), Is.False);
        }
        else
        {
            await ((WeakActionAsync<int>)action).InvokeAsync(42);
        }

        Assert.That(counter.Value, Is.Zero);
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task ClosedStatic_InvokesWithLiveDerivedTarget(bool asynchronous)
    {
        var target = new Counter();
        if (asynchronous)
        {
            var handler = GetMethod(nameof(HandleAsync)).CreateDelegate<Func<int, ValueTask>>(target);
            var action = new WeakActionAsync<int>(handler);
            Assert.That(action.Matches(handler), Is.True);
            await action.InvokeAsync(42);
        }
        else
        {
            var handler = GetMethod(nameof(Handle)).CreateDelegate<Action<int>>(target);
            var action = new WeakAction<int>(handler);
            Assert.That(action.Matches(handler), Is.True);
            Assert.That(action.TryInvoke(42), Is.True);
        }

        Assert.That(target.Value, Is.EqualTo(42));
        GC.KeepAlive(target);
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task ClosedStatic_NullBoundArgumentIsPreserved(bool asynchronous)
    {
        _nullCalls = 0;
        if (asynchronous)
        {
            var handler = GetMethod(nameof(HandleNullAsync)).CreateDelegate<Func<int, ValueTask>>(null);
            await new WeakActionAsync<int>(handler).InvokeAsync(42);
        }
        else
        {
            var handler = GetMethod(nameof(HandleNull)).CreateDelegate<Action<int>>(null);
            Assert.That(new WeakAction<int>(handler).TryInvoke(42), Is.True);
        }

        Assert.That(_nullCalls, Is.EqualTo(1));
    }

    private static int _nullCalls;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static (object Action, WeakReference Target, Counter Counter) CreateAction(bool asynchronous)
    {
        var counter = new Counter();
        var target = new Forwarder(counter);
        object action = asynchronous
            ? new WeakActionAsync<int>(GetMethod(nameof(HandleAsync)).CreateDelegate<Func<int, ValueTask>>(target))
            : new WeakAction<int>(GetMethod(nameof(Handle)).CreateDelegate<Action<int>>(target));
        return (action, new WeakReference(target), counter);
    }

    private static MethodInfo GetMethod(string name) =>
        typeof(WeakActionClosedStaticTests).GetMethod(name, BindingFlags.Static | BindingFlags.NonPublic)!;

    private static void Handle(ITarget target, int value) => target.Add(value);

    private static ValueTask HandleAsync(ITarget target, int value)
    {
        target.Add(value);
        return ValueTask.CompletedTask;
    }

    private static void HandleNull(ITarget? target, int value)
    {
        if (target is null && value == 42)
            _nullCalls++;
    }

    private static ValueTask HandleNullAsync(ITarget? target, int value)
    {
        HandleNull(target, value);
        return ValueTask.CompletedTask;
    }

    private interface ITarget
    {
        void Add(int value);
    }

    private sealed class Counter : ITarget
    {
        internal int Value { get; private set; }
        public void Add(int value) => Value += value;
    }

    private sealed class Forwarder(Counter counter) : ITarget
    {
        public void Add(int value) => counter.Add(value);
    }
}
