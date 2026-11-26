using Messager.NET.Entities;

namespace Messager.NET.Tests.Entities.Exchanges;

public sealed partial class Exchange_Tests
{
    [Test]
    public void MultipleSubscribers_ShouldAllReceiveMessage()
    {
        var exchange = new Exchange();
        var sender = exchange.GetSender<string>();
        var receiver = exchange.GetReceiver<string>();

        var received1 = false;
        var received2 = false;
        var received3 = false;

        using (receiver.Subscribe(_ => received1 = true))
        using (receiver.Subscribe(_ => received2 = true))
        using (receiver.Subscribe(_ => received3 = true))
            sender.Send("Test");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(received1, Is.True);
            Assert.That(received2, Is.True);
            Assert.That(received3, Is.True);
        }
    }

    [Test]
    public void Unsubscribe_ShouldStopReceivingMessages()
    {
        var exchange = new Exchange();
        var sender = exchange.GetSender<string>();
        var receiver = exchange.GetReceiver<string>();

        var receivedCount = 0;

        var subscription = receiver.Subscribe(_ => receivedCount++);

        sender.Send("Test1");
        Assert.That(receivedCount, Is.EqualTo(1));

        subscription.Dispose();
        sender.Send("Test2");
        Assert.That(receivedCount, Is.EqualTo(1));
    }

    [Test]
    public void KeyedUnsubscribe_ShouldStopReceivingMessages()
    {
        var exchange = new Exchange();
        var sender = exchange.GetKeyedSender<string, int>();
        var receiver = exchange.GetKeyedReceiver<string, int>();

        var receivedCount = 0;

        var subscription = receiver.Subscribe("key1", _ => receivedCount++);

        sender.Send("key1", 1);
        Assert.That(receivedCount, Is.EqualTo(1));

        subscription.Dispose();
        sender.Send("key1", 2);
        Assert.That(receivedCount, Is.EqualTo(1));
    }
}
