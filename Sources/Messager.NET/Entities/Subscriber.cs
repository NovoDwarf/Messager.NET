using Messager.NET.Receivers;

namespace Messager.NET.Entities;

internal sealed class Subscriber<TEvent>
{
    private readonly Action<TEvent>? _strongHandler;
    private readonly WeakAction<TEvent>? _weakHandler;

    internal Subscriber(Action<TEvent> handler, SubscriptionReference reference)
    {
        ArgumentNullException.ThrowIfNull(handler);

        switch (reference)
        {
            case SubscriptionReference.Strong: _strongHandler = handler; break;
            case SubscriptionReference.Weak: _weakHandler = new WeakAction<TEvent>(handler); break;
            default: throw new ArgumentOutOfRangeException(nameof(reference), reference, "Unknown reference mode.");
        }
    }

    internal bool IsAlive => _strongHandler is not null || _weakHandler?.IsAlive == true;

    internal bool Matches(Action<TEvent> handler)
    {
        ArgumentNullException.ThrowIfNull(handler);

        return _strongHandler is not null
            ? _strongHandler == handler
            : _weakHandler!.Matches(handler);
    }

    internal bool Invoke(TEvent evt)
    {
        if (_strongHandler is not null)
        {
            _strongHandler(evt);
            return true;
        }

        return _weakHandler!.TryInvoke(evt);
    }
}
