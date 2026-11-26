using Messager.NET.Entities;

namespace Messager.NET.Tests.Entities.Unsubscribers;

[TestFixture]
public sealed partial class Unsubscriber_Tests
{
    [Test]
    public void Constructor_WithValidAction_ShouldInitialize()
    {
        var action = new Action(() => { });

        var unsubscriber = new Unsubscriber(action);

        Assert.That(unsubscriber.IsDisposed, Is.False);
    }

    [Test]
    public void Constructor_WithNullAction_ShouldThrowArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => { var unsubscriber = new Unsubscriber(null!); });
    }

    [Test]
    public void IsDisposed_ShouldReflectCorrectState()
    {
        var unsubscriber = new Unsubscriber(() => { });

        Assert.That(unsubscriber.IsDisposed, Is.False);

        unsubscriber.Dispose();
        Assert.That(unsubscriber.IsDisposed, Is.True);
    }



    [Test]
    public void MultipleInstances_ShouldWorkIndependently()
    {
        var callCount1 = 0;
        var callCount2 = 0;

        var unsubscriber1 = new Unsubscriber(() => callCount1++);
        var unsubscriber2 = new Unsubscriber(() => callCount2++);

        unsubscriber1.Dispose();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(unsubscriber1.IsDisposed, Is.True);
            Assert.That(callCount1, Is.EqualTo(1));
            Assert.That(unsubscriber2.IsDisposed, Is.False);
            Assert.That(callCount2, Is.Zero);
        }
    }

}
