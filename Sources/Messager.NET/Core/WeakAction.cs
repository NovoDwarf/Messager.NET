using System.Reflection;

namespace Messager.NET.Core;

internal class WeakAction<T>
{
	private readonly WeakReference? _targetRef;
	private readonly MethodInfo _method;
	private readonly bool _isStatic;

	internal WeakAction(Action<T> action)
	{
		_method = action.Method;
		_isStatic = _method.IsStatic;

		_targetRef = _isStatic ? null : new WeakReference(action.Target!);
	}

	internal bool IsAlive => _isStatic || (_targetRef?.IsAlive ?? false);

	internal bool TryInvoke(T arg)
	{
		if (!_isStatic && !IsAlive)
			return false;

		var target = _isStatic ? null : _targetRef!.Target;
		
		if (!_isStatic && target == null)
			return false;

		_method.Invoke(target, [arg]);
		return true;
	}

	internal bool Matches(Action<T> action)
	{
		if (action.Method != _method) 
			return false;
		
		return _isStatic || ReferenceEquals(action.Target, _targetRef!.Target);
	}
}