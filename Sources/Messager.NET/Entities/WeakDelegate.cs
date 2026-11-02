using System.Reflection;

namespace Messager.NET.Entities;

internal sealed class WeakDelegate<TDelegate> where TDelegate : Delegate
{
    private readonly Invocation[] _invocations;

    internal WeakDelegate(TDelegate handler)
    {
        var callbacks = handler.GetInvocationList();
       
        _invocations = new Invocation[callbacks.Length];

        for (var i = 0; i < callbacks.Length; i++)
            _invocations[i] = new Invocation(callbacks[i]);
    }

    internal int Count => _invocations.Length;

    internal bool IsAlive => _invocations.Any(invocation => invocation.IsAlive);

    internal TDelegate? GetCallback(int index) => _invocations[index].GetCallback();

    internal bool Matches(TDelegate handler)
    {
        var callbacks = handler.GetInvocationList();

        if (callbacks.Length != _invocations.Length)
            return false;
        
        for (var i = 0; i < callbacks.Length; i++)
        {
            if (!_invocations[i].Matches(callbacks[i]))
            {
                return false;
            }
        }

        return true;
    }

    private sealed class Invocation
    {
        private readonly WeakReference? _targetRef;
        private readonly MethodInfo _method;

        internal Invocation(Delegate callback)
        {
            _method = callback.Method;
            _targetRef = callback.Target is { } target ? new WeakReference(target) : null;
        }

        internal bool IsAlive => _targetRef == null || _targetRef.IsAlive;

        internal TDelegate? GetCallback()
        {
            var target = _targetRef?.Target;

            if (_targetRef != null && target == null)
                return null;
            
            return _targetRef == null
                ? _method.CreateDelegate<TDelegate>()
                : _method.CreateDelegate<TDelegate>(target);
        }

        internal bool Matches(Delegate callback)
        {
            return callback.Method == _method && ReferenceEquals(callback.Target, _targetRef?.Target);
        }
    }
}
