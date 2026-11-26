using Messager.NET.Entities;
using Messager.NET.Factories;
using Messager.NET.Receivers;
using Messager.NET.Senders;

namespace Messager.NET.Tests.Entities.Exchanges;

[TestFixture]
public sealed partial class Exchange_Tests
{
    [Test]
    public void Constructor_WithoutLoggerFactory_ShouldCreateInstance()
    {
        var exchange = new Exchange();

        Assert.That(exchange, Is.Not.Null);
        Assert.That(exchange, Is.InstanceOf<IBrokerFactory>());
        Assert.That(exchange, Is.InstanceOf<IKeyedBrokerFactory>());
    }

    [Test]
    public void GetSender_ShouldReturnNotNull()
    {
        var exchange = new Exchange();
        var sender = exchange.GetSender<string>();

        Assert.That(sender, Is.Not.Null);
        Assert.That(sender, Is.InstanceOf<ISender<string>>());
    }

    [Test]
    public void GetReceiver_ShouldReturnNotNull()
    {
        var exchange = new Exchange();
        var receiver = exchange.GetReceiver<string>();

        Assert.That(receiver, Is.Not.Null);
        Assert.That(receiver, Is.InstanceOf<IReceiver<string>>());
    }

    [Test]
    public void GetSender_And_GetReceiver_ShouldReturnSameBroker()
    {
        var exchange = new Exchange();
        var sender = exchange.GetSender<int>();
        var receiver = exchange.GetReceiver<int>();

        Assert.That(sender, Is.SameAs(receiver));
    }

    [Test]
    public void GetSender_ForDifferentTypes_ShouldReturnDifferentBrokers()
    {
        var exchange = new Exchange();
        var sender1 = exchange.GetSender<string>();
        var sender2 = exchange.GetSender<int>();

        Assert.That(sender1, Is.Not.SameAs(sender2));
    }

    [Test]
    public void GetSender_ForSameType_ShouldReturnSameInstance()
    {
        var exchange = new Exchange();
        var sender1 = exchange.GetSender<string>();
        var sender2 = exchange.GetSender<string>();

        Assert.That(sender1, Is.SameAs(sender2));
    }

    [Test]
    public void Send_And_Receive_ShouldWork()
    {
        var exchange = new Exchange();
        var sender = exchange.GetSender<string>();
        var receiver = exchange.GetReceiver<string>();

        var receivedValue = string.Empty;
        using (receiver.Subscribe(msg => receivedValue = msg))
            sender.Send("Test Message");

        Assert.That(receivedValue, Is.EqualTo("Test Message"));
    }
}
