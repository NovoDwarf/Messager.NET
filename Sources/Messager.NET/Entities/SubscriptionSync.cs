using Messager.NET.Receivers;

namespace Messager.NET.Entities;

internal sealed class SubscriptionSync<TEvent>
{
    private readonly Action<TEvent>? _strongHandler;
    private readonly WeakAction<TEvent>? _weakHandler;

    internal SubscriptionSync(Action<TEvent> handler, SubscriptionReference reference)
    {
        switch (reference)
        {
            case SubscriptionReference.Strong:
                _strongHandler = handler;
                break;
            case SubscriptionReference.Weak:
                _weakHandler = new WeakAction<TEvent>(handler);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(reference), reference, "Unknown reference mode.");
        }
    }

    internal bool IsAlive => _strongHandler != null || _weakHandler!.IsAlive;

    internal void Invoke(TEvent evt)
    {
        if (_strongHandler != null)
        {
            _strongHandler(evt);
            return;
        }

        _weakHandler!.TryInvoke(evt);
    }
}
