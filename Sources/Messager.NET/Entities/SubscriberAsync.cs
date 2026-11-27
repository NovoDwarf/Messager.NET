using Messager.NET.Receivers;

namespace Messager.NET.Entities;

internal sealed class SubscriberAsync<TEvent>
{
	private readonly Func<TEvent, ValueTask>? _strongHandler;
	private readonly Delegate[]? _callbacks;
	private readonly WeakActionAsync<TEvent>? _weakHandler;

	internal SubscriberAsync(Func<TEvent, ValueTask> handler, SubscriptionReference reference)
	{
		ArgumentNullException.ThrowIfNull(handler);

		switch (reference)
		{
			case SubscriptionReference.Strong:
				_strongHandler = handler;
				_callbacks = handler.GetInvocationList();
				break;
			case SubscriptionReference.Weak: _weakHandler = new WeakActionAsync<TEvent>(handler); break;
			default: throw new ArgumentOutOfRangeException(nameof(reference), reference, "Unknown reference mode.");
		}
	}

	internal bool IsAlive => _strongHandler is not null || _weakHandler?.IsAlive == true;

	internal bool Matches(Func<TEvent, ValueTask> handler)
	{
		ArgumentNullException.ThrowIfNull(handler);

		return _strongHandler is not null
			? _strongHandler == handler
			: _weakHandler!.Matches(handler);
	}

	internal async ValueTask InvokeAsync(TEvent evt)
	{
		if (_callbacks is not null)
		{
			foreach (var callback in _callbacks)
				await ((Func<TEvent, ValueTask>)callback)(evt).ConfigureAwait(false);

			return;
		}

		await _weakHandler!.InvokeAsync(evt).ConfigureAwait(false);
	}
}