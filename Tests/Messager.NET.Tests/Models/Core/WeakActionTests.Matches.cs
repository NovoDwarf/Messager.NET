using Messager.NET.Core;

namespace Messager.NET.Tests.Models.Core;

public partial class WeakActionTests
{
	[Test]
	public void Matches_ShouldReturnTrue_ForSameStaticAction()
	{
		var action1 = StaticMethod;
		var action2 = StaticMethod;
		var weakAction = new WeakAction<string>(action1);

		var result = weakAction.Matches(action2);

		Assert.That(result, Is.True);
	}

	[Test]
	public void Matches_ShouldReturnTrue_ForSameInstanceAction()
	{
		var instance = new TestClass();
		var action1 = instance.InstanceMethod;
		var action2 = instance.InstanceMethod;
		var weakAction = new WeakAction<string>(action1);

		var result = weakAction.Matches(action2);

		Assert.That(result, Is.True);
	}

	[Test]
	public void Matches_ShouldReturnFalse_ForDifferentMethods()
	{
		var instance = new TestClass();
		var action1 = instance.InstanceMethod;
		var action2 = instance.AnotherMethod;
		var weakAction = new WeakAction<string>(action1);

		var result = weakAction.Matches(action2);

		Assert.That(result, Is.False);
	}

	[Test]
	public void Matches_ShouldReturnFalse_ForDifferentInstances()
	{
		var instance1 = new TestClass();
		var instance2 = new TestClass();

		var action1 = instance1.InstanceMethod;
		var action2 = instance2.InstanceMethod;

		var weakAction = new WeakAction<string>(action1);

		var result = weakAction.Matches(action2);

		Assert.That(result, Is.False);
	}

	[Test]
	public void Matches_ShouldReturnFalse_ForDifferentDelegateTypes()
	{
		var instance = new TestClass();
		var action1 = instance.InstanceMethod;
		var action2 = SomeOtherStaticMethod;
		var weakAction = new WeakAction<string>(action1);

		var result = weakAction.Matches(action2);

		Assert.That(result, Is.False);
	}

	[Test]
	public void Matches_ShouldHandleNullArgument()
	{
		var weakAction = new WeakAction<string>(Action);

		Assert.That(() => weakAction.Matches(null!), Throws.Exception);

		return;

		void Action(string value)
		{
		}
	}
}