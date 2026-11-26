using Messager.NET.Entities;
using static Messager.NET.Tests.Helpers.TestHelpers;

namespace Messager.NET.Tests.Entities.WeakActions;

[TestFixture]
public sealed partial class WeakAction_Tests
{
    [Test]
    public void Constructor_ShouldInitializeStaticAction()
    {
        var staticAction = StaticMethod;

        var weakAction = new WeakAction<string>(staticAction);

        Assert.That(weakAction.IsAlive, Is.True);
    }

    [Test]
    public void Constructor_ShouldInitializeInstanceAction()
    {
        var instance = new WeakActionTestTarget();
        var instanceAction = instance.InstanceMethod;

        var weakAction = new WeakAction<string>(instanceAction);

        Assert.That(weakAction.IsAlive, Is.True);
    }

    [Test]
    public void IsAlive_ShouldReturnTrue_ForStaticAction()
    {
        var staticAction = StaticMethod;
        var weakAction = new WeakAction<string>(staticAction);

        Assert.That(weakAction.IsAlive, Is.True);
    }

    [Test]
    public void IsAlive_ShouldReturnTrue_ForLiveInstance()
    {
        var instance = new WeakActionTestTarget();
        var instanceAction = instance.InstanceMethod;
        var weakAction = new WeakAction<string>(instanceAction);

        Assert.That(weakAction.IsAlive, Is.True);
    }

    [Test]
    public void WeakAction_ShouldNotPreventGarbageCollection()
    {
        WeakReference weakRef = null!;
        var weakAction = CreateWeakActionWithCollectableTarget(ref weakRef);

        CollectGarbage();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(weakRef.IsAlive, Is.False);
            Assert.That(weakAction.IsAlive, Is.False);
        }
    }

    [Test]
    public void WeakAction_ShouldWorkWithLambdaExpressions()
    {
        const string capturedVariable = "captured";
        string receivedValue = null!;

        var weakAction = new WeakAction<string>(Action);
        var result = weakAction.TryInvoke("test_");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Is.True);
            Assert.That(receivedValue, Is.EqualTo("test_captured"));
        }

        return;

        void Action(string value) => receivedValue = value + capturedVariable;
    }

    [Test]
    public void WeakAction_ShouldWorkWithAnonymousMethods()
    {
        string receivedValue = null!;

        var weakAction = new WeakAction<string>(Anonymous);
        var result = weakAction.TryInvoke("hello");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Is.True);
            Assert.That(receivedValue, Is.EqualTo("HELLO"));
        }

        return;

        void Anonymous(string value)
        {
            receivedValue = value.ToUpperInvariant();
        }
    }

    private static void StaticMethod(string value)
    {

    }

    private static void SomeOtherStaticMethod(string value)
    {

    }

    private WeakAction<string> CreateWeakActionForCollectableInstance()
    {
        var instance = new WeakActionTestTarget();
        var action = instance.InstanceMethod;

        return new WeakAction<string>(action);
    }

    // ReSharper disable once RedundantAssignment
    private WeakAction<string> CreateWeakActionWithCollectableTarget(ref WeakReference weakRef)
    {
        var temporaryInstance = new WeakActionTestTarget();
        weakRef = new WeakReference(temporaryInstance);

        var action = temporaryInstance.InstanceMethod;
        var weakAction = new WeakAction<string>(action);

        temporaryInstance = null;
        return weakAction;
    }
}
