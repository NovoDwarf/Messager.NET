using System.Reflection;

namespace Messager.NET.Entities;

internal sealed class WeakActionAsync<T>
{
	private readonly Invocation[] _invocations;

	internal WeakActionAsync(Func<T, ValueTask> handler)
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

	internal bool Matches(Func<T, ValueTask> handler)
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

	internal async ValueTask InvokeAsync(T arg)
	{
		foreach (var invocation in _invocations)
		{
			await invocation.InvokeAsync(arg).ConfigureAwait(false);
		}
	}

	private sealed class Invocation
	{
		private readonly WeakInvocation _invocation;
		private readonly IInvoker? _invoker;

		internal Invocation(Delegate callback)
		{
			_invocation = new WeakInvocation(callback);

			if (_invocation.TryGetStaticHandler<Func<T, ValueTask>>(out _))
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

		internal bool Matches(Delegate callback) => _invocation.Matches(callback);

		internal ValueTask InvokeAsync(T arg)
		{
			if (_invocation.TryGetStaticHandler<Func<T, ValueTask>>(out var handler))
				return handler!(arg);

			if (!_invocation.TryGetTarget(out var target))
				return ValueTask.CompletedTask;

			return InvokeWithLifetimeAsync(target!, arg);
		}

		private async ValueTask InvokeWithLifetimeAsync(object target, T arg)
		{
			try
			{
				await _invoker!.InvokeAsync(target, arg).ConfigureAwait(false);
			}
			finally
			{
				GC.KeepAlive(target);
			}
		}
	}

	private interface IInvoker
	{
		ValueTask InvokeAsync(object target, T arg);
	}

	private sealed class Invoker<TTarget> : IInvoker where TTarget : class
	{
		private readonly Func<TTarget, T, ValueTask> _handler;

		public Invoker(MethodInfo method)
		{
			_handler = method.CreateDelegate<Func<TTarget, T, ValueTask>>();
		}

		public ValueTask InvokeAsync(object target, T arg)
		{
			return _handler((TTarget)target, arg);
		}
	}
}
