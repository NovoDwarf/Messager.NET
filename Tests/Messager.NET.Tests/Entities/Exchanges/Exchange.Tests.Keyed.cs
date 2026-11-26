using Messager.NET.Entities;
using Messager.NET.Receivers;
using Messager.NET.Senders;

namespace Messager.NET.Tests.Entities.Exchanges;

public sealed partial class Exchange_Tests
{
    [Test]
    public void GetKeyedSender_ShouldReturnNotNull()
    {
        var exchange = new Exchange();
        var sender = exchange.GetKeyedSender<string, int>();

        Assert.That(sender, Is.Not.Null);
        Assert.That(sender, Is.InstanceOf<ISender<string, int>>());
    }

    [Test]
    public void GetKeyedReceiver_ShouldReturnNotNull()
    {
        var exchange = new Exchange();
        var receiver = exchange.GetKeyedReceiver<string, int>();

        Assert.That(receiver, Is.Not.Null);
        Assert.That(receiver, Is.InstanceOf<IReceiver<string, int>>());
    }

    [Test]
    public void GetKeyedSender_And_GetKeyedReceiver_ShouldReturnSameBroker()
    {
        var exchange = new Exchange();
        var sender = exchange.GetKeyedSender<string, int>();
        var receiver = exchange.GetKeyedReceiver<string, int>();

        Assert.That(sender, Is.SameAs(receiver));
    }

    [Test]
    public void GetKeyedSender_ForDifferentTypes_ShouldReturnDifferentBrokers()
    {
        var exchange = new Exchange();
        var sender1 = exchange.GetKeyedSender<string, int>();
        var sender2 = exchange.GetKeyedSender<int, string>();

        Assert.That(sender1, Is.Not.SameAs(sender2));
    }

    [Test]
    public void GetKeyedSender_ForSameTypes_ShouldReturnSameInstance()
    {
        var exchange = new Exchange();
        var sender1 = exchange.GetKeyedSender<string, int>();
        var sender2 = exchange.GetKeyedSender<string, int>();

        Assert.That(sender1, Is.SameAs(sender2));
    }

    [Test]
    public void SendKeyed_And_ReceiveKeyed_ShouldWork()
    {
        var exchange = new Exchange();
        var sender = exchange.GetKeyedSender<string, int>();
        var receiver = exchange.GetKeyedReceiver<string, int>();

        var receivedValue = 0;
        using (receiver.Subscribe("key1", msg => receivedValue = msg))
            sender.Send("key1", 42);

        Assert.That(receivedValue, Is.EqualTo(42));
    }

    [Test]
    public void SendKeyed_WithDifferentKeys_ShouldDeliverToCorrectReceiver()
    {
        var exchange = new Exchange();
        var sender = exchange.GetKeyedSender<string, int>();
        var receiver = exchange.GetKeyedReceiver<string, int>();

        var value1 = 0;
        var value2 = 0;

        using (receiver.Subscribe("key1", msg => value1 = msg))
        using (receiver.Subscribe("key2", msg => value2 = msg))
        {
            sender.Send("key1", 10);
            sender.Send("key2", 20);
        }

        using (Assert.EnterMultipleScope())
        {
            Assert.That(value1, Is.EqualTo(10));
            Assert.That(value2, Is.EqualTo(20));
        }
    }
}
