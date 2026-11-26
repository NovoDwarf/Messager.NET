using Messager.NET.Entities;
using Messager.NET.Receivers;
using Messager.NET.Senders;

namespace Messager.NET.Tests.Entities.Exchanges;

public sealed partial class Exchange_Tests
{
    [Test]
    public void GetAsyncKeyedSender_ShouldReturnNotNull()
    {
        var exchange = new Exchange();
        var sender = exchange.GetAsyncKeyedSender<string, int>();

        Assert.That(sender, Is.Not.Null);
        Assert.That(sender, Is.InstanceOf<IAsyncSender<string, int>>());
    }

    [Test]
    public void GetAsyncKeyedReceiver_ShouldReturnNotNull()
    {
        var exchange = new Exchange();
        var receiver = exchange.GetAsyncKeyedReceiver<string, int>();

        Assert.That(receiver, Is.Not.Null);
        Assert.That(receiver, Is.InstanceOf<IAsyncReceiver<string, int>>());
    }

    [Test]
    public void GetAsyncKeyedSender_And_GetAsyncKeyedReceiver_ShouldReturnSameBroker()
    {
        var exchange = new Exchange();
        var sender = exchange.GetAsyncKeyedSender<string, int>();
        var receiver = exchange.GetAsyncKeyedReceiver<string, int>();

        Assert.That(sender, Is.SameAs(receiver));
    }

    [Test]
    public async Task SendAsyncKeyed_And_ReceiveAsyncKeyed_ShouldWork()
    {
        var exchange = new Exchange();
        var sender = exchange.GetAsyncKeyedSender<string, int>();
        var receiver = exchange.GetAsyncKeyedReceiver<string, int>();

        var receivedValue = 0;

        await using (receiver.Subscribe("key1", async msg => { receivedValue = msg; await Task.CompletedTask; }))
            await sender.SendAsync("key1", 100);

        Assert.That(receivedValue, Is.EqualTo(100));
    }

    [Test]
    public async Task SendAsyncKeyed_WithDifferentKeys_ShouldDeliverToCorrectReceiver()
    {
        var exchange = new Exchange();
        var sender = exchange.GetAsyncKeyedSender<string, int>();
        var receiver = exchange.GetAsyncKeyedReceiver<string, int>();

        var value1 = 0;
        var value2 = 0;

        await using (receiver.Subscribe("key1", async msg => { value1 = msg; await Task.CompletedTask; }))
        await using (receiver.Subscribe("key2", async msg => { value2 = msg; await Task.CompletedTask; }))
        {
            await sender.SendAsync("key1", 30);
            await sender.SendAsync("key2", 40);
        }

        using (Assert.EnterMultipleScope())
        {
            Assert.That(value1, Is.EqualTo(30));
            Assert.That(value2, Is.EqualTo(40));
        }
    }
}
