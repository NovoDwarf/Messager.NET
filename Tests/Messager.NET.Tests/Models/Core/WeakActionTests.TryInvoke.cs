using System.Reflection;
using Messager.NET.Core;

namespace Messager.NET.Tests.Models.Core;

public partial class WeakActionTests
{
	[Test]
	public void TryInvoke_ShouldInvokeStaticMethod()
	{
		string receivedValue = null!;
		var weakAction = new WeakAction<string>(Action);

		var result = weakAction.TryInvoke("TestValue");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Is.True);
            Assert.That(receivedValue, Is.EqualTo("TestValue"));
        }

        return;

        void Action(string value) => receivedValue = value;
	}

	[Test]
	public void TryInvoke_ShouldInvokeInstanceMethod()
	{
		var instance = new TestClass();
		var weakAction = new WeakAction<string>(instance.InstanceMethod);

		var result = weakAction.TryInvoke("InstanceValue");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Is.True);
            Assert.That(instance.LastValue, Is.EqualTo("InstanceValue"));
        }
    }

	[Test]
	public void TryInvoke_ShouldReturnFalse_ForCollectedInstance()
	{
		var weakAction = CreateWeakActionForCollectableInstance();

		GC.Collect();
		GC.WaitForPendingFinalizers();
		GC.Collect();

		var result = weakAction.TryInvoke("TestValue");

		Assert.That(result, Is.False);
	}

	[Test]
	public void TryInvoke_ShouldWorkWithValueTypes()
	{
		var receivedValue = 0;
		var weakAction = new WeakAction<int>(Action);

		var result = weakAction.TryInvoke(42);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Is.True);
            Assert.That(receivedValue, Is.EqualTo(42));
        }

        return;
		
		void Action(int value) => receivedValue = value;
	}

	[Test]
	public void TryInvoke_ShouldWorkWithComplexTypes()
	{
		TestClass receivedInstance = null!;

		var weakAction = new WeakAction<TestClass>(Action);
		var testInstance = new TestClass();

		var result = weakAction.TryInvoke(testInstance);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Is.True);
            Assert.That(receivedInstance, Is.SameAs(testInstance));
        }

        return;

        void Action(TestClass instance) => receivedInstance = instance;
	}

	[Test]
	public void TryInvoke_ShouldWorkWithNullArguments()
	{
		var receivedValue = "not_null";
		var weakAction = new WeakAction<string>(Action);

		var result = weakAction.TryInvoke(null!);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Is.True);
            Assert.That(receivedValue, Is.Null);
        }
        
        return;
        
		void Action(string value) => receivedValue = value;
	}
	
	
	[Test]
	public void TryInvoke_ShouldFail_AfterTargetIsCollected()
	{
		WeakReference weakRef = null!;
		var weakAction = CreateWeakActionWithCollectableTarget(ref weakRef);
		var invocationCount = 0;

		var firstResult = weakAction.TryInvoke("First");
	    
		if (firstResult) 
			invocationCount++;

		GC.Collect();
		GC.WaitForPendingFinalizers();
		GC.Collect();

		var secondResult = weakAction.TryInvoke("Second");
	   
		if (secondResult) 
			invocationCount++;

		using (Assert.EnterMultipleScope())
		{
			Assert.That(weakRef.IsAlive, Is.False);
			Assert.That(firstResult, Is.True);
			Assert.That(secondResult, Is.False);
			Assert.That(invocationCount, Is.EqualTo(1));
		}
	}
    
	[Test]
	public void TryInvoke_ShouldHandleMethodWithExceptions()
	{
		var weakAction = new WeakAction<string>(Action);

		Assert.Throws<TargetInvocationException>(() => weakAction.TryInvoke("Test"));
	    
		return;

		void Action(string value) => throw new InvalidOperationException("Test exception");
	}
}