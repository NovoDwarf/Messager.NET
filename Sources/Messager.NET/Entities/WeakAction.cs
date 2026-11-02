using System.Reflection;

namespace Messager.NET.Entities;

internal sealed class WeakAction<T>
{
    private readonly WeakDelegate<Action<T>> _handler;

    internal WeakAction(Action<T> action)
    {
        _handler = new WeakDelegate<Action<T>>(action);
    }

    internal bool IsAlive => _handler.IsAlive;

    internal bool TryInvoke(T arg)
    {
        var invoked = false;

        for (var i = 0; i < _handler.Count; i++)
        {
            var callback = _handler.GetCallback(i);

            if (callback == null)
                continue;
            
            try
            {
                callback(arg);
            }
            catch (Exception exception)
            {
                throw new TargetInvocationException(exception);
            }

            invoked = true;
        }

        return invoked;
    }

    internal bool Matches(Action<T> action) => _handler.Matches(action);
}
