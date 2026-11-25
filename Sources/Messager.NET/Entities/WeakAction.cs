using System.Reflection;

namespace Messager.NET.Entities;

internal sealed class WeakAction<T>
{
    private readonly Invocation[] _invocations;

    internal WeakAction(Action<T> handler)
    {
        ArgumentNullException.ThrowIfNull(handler);

        var callbacks = handler.GetInvocationList();
        _invocations = new Invocation[callbacks.Length];

        for (var i = 0; i < callbacks.Length; i++)
            _invocations[i] = new Invocation(callbacks[i]);
    }

    internal bool IsAlive
    {
        get
        {
            foreach (var invocation in _invocations)
            {
                if (invocation.IsAlive)
                    return true;
            }

            return false;
        }
    }

    internal bool TryInvoke(T arg)
    {
        var invoked = false;

        foreach (var invocation in _invocations)
        {
            try
            {
                if (invocation.TryInvoke(arg))
                    invoked = true;
            }
            catch (Exception exception)
            {
                throw new TargetInvocationException(exception);
            }
        }

        return invoked;
    }

    internal bool Matches(Action<T> handler)
    {
        ArgumentNullException.ThrowIfNull(handler);

        var callbacks = handler.GetInvocationList();

        if (callbacks.Length != _invocations.Length)
            return false;

        for (var i = 0; i < callbacks.Length; i++)
        {
            if (!_invocations[i].Matches(callbacks[i]))
                return false;
        }

        return true;
    }

    private sealed class Invocation
    {
        private readonly WeakInvocation _invocation;
        private readonly IInvoker? _invoker;

        internal Invocation(Delegate callback)
        {
            _invocation = new WeakInvocation(callback);

            if (_invocation.TryGetStaticHandler<Action<T>>(out _))
                return;

            var method = _invocation.Method;

            var targetType = method.IsStatic
                ? method.GetParameters()[0].ParameterType
                : callback.Target!.GetType();

            if (targetType.IsValueType)
                throw new NotSupportedException("Value-type targets are not supported.");

            var invokerType = typeof(Invoker<>).MakeGenericType(typeof(T), targetType);

            _invoker = (IInvoker)Activator.CreateInstance(invokerType, method)!;
        }

        internal bool IsAlive => _invocation.IsAlive;

        internal bool Matches(Delegate callback) =>
            _invocation.Matches(callback);

        internal bool TryInvoke(T arg)
        {
            if (_invocation.TryGetStaticHandler<Action<T>>(out var handler))
            {
                handler!(arg);
                return true;
            }

            if (!_invocation.TryGetTarget(out var target))
                return false;

            _invoker!.Invoke(target!, arg);

            GC.KeepAlive(target);
            return true;
        }
    }

    private interface IInvoker
    {
        void Invoke(object target, T arg);
    }

    private sealed class Invoker<TTarget> : IInvoker where TTarget : class
    {
        private readonly Action<TTarget, T> _handler;

        public Invoker(MethodInfo method)
        {
            _handler = method.CreateDelegate<Action<TTarget, T>>();
        }

        public void Invoke(object target, T arg)
        {
            _handler((TTarget)target, arg);
        }
    }
}
