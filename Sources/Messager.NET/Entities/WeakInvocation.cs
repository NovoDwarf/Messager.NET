using System.Reflection;

namespace Messager.NET.Entities;

internal sealed class WeakInvocation
{
	private readonly WeakReference<object>? _target;
	private readonly Delegate? _staticHandler;

	internal WeakInvocation(Delegate handler)
	{
		ArgumentNullException.ThrowIfNull(handler);

		Method = handler.Method;

		if (handler.Target is { } target)
			_target = new WeakReference<object>(target);
		else
			_staticHandler = handler;
	}

	internal MethodInfo Method { get; }

	internal bool IsAlive => _target is null || _target.TryGetTarget(out _);

	internal bool TryGetTarget(out object? target)
	{
		if (_target is not null)
			return _target.TryGetTarget(out target);
        
		target = null;
		return true;
	}

	internal bool TryGetStaticHandler<TDelegate>(out TDelegate? handler) where TDelegate : Delegate
	{
		handler = _staticHandler as TDelegate;
		return handler is not null;
	}

	internal bool Matches(Delegate handler)
	{
		ArgumentNullException.ThrowIfNull(handler);

		if (handler.Method != Method)
			return false;

		if (_target is null)
			return handler.Target is null;

		return _target.TryGetTarget(out var target) && ReferenceEquals(target, handler.Target);
	}
}