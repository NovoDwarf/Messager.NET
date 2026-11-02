using Messager.NET.Receivers;

namespace Messager.NET.Entities;

internal sealed class SubscriptionAsync<TEvent>
{
    private readonly Func<TEvent, ValueTask>? _strongHandler;
    private readonly Delegate[]? _callbacks;
    private readonly WeakDelegate<Func<TEvent, ValueTask>>? _weakHandler;

    internal SubscriptionAsync(Func<TEvent, ValueTask> handler, SubscriptionReference reference)
    {
        ArgumentNullException.ThrowIfNull(handler);

        switch (reference)
        {
            case SubscriptionReference.Strong:
                _strongHandler = handler;
                _callbacks = handler.GetInvocationList();
                break;
            case SubscriptionReference.Weak:
                _weakHandler = new WeakDelegate<Func<TEvent, ValueTask>>(handler);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(reference), reference, "Unknown reference mode.");
        }
    }

    internal bool IsAlive => _strongHandler != null || _weakHandler!.IsAlive;

    internal bool Matches(Func<TEvent, ValueTask> handler)
    {
        return _strongHandler != null 
            ? _strongHandler == handler 
            : _weakHandler!.Matches(handler);
    }

    internal async ValueTask InvokeAsync(TEvent evt)
    {
        if (_callbacks != null)
        {
            foreach (var callback in _callbacks)
                await ((Func<TEvent, ValueTask>)callback)(evt).ConfigureAwait(false);
            
            return;
        }

        for (var i = 0; i < _weakHandler!.Count; i++)
        {
            var callback = _weakHandler.GetCallback(i);

            if (callback != null)
                await callback(evt).ConfigureAwait(false);
        }
    }
}
