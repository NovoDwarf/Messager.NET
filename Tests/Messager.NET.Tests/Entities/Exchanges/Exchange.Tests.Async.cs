using Messager.NET.Entities;
using Messager.NET.Receivers;
using Messager.NET.Senders;

namespace Messager.NET.Tests.Entities.Exchanges;

public sealed partial class Exchange_Tests
{
    [Test]
    public void GetAsyncSender_ShouldReturnNotNull()
    {
        var exchange = new Exchange();
        var sender = exchange.GetAsyncSender<string>();

        Assert.That(sender, Is.Not.Null);
        Assert.That(sender, Is.InstanceOf<IAsyncSender<string>>());
    }

    [Test]
    public void GetAsyncReceiver_ShouldReturnNotNull()
    {
        var exchange = new Exchange();
        var receiver = exchange.GetAsyncReceiver<string>();

        Assert.That(receiver, Is.Not.Null);
        Assert.That(receiver, Is.InstanceOf<IAsyncReceiver<string>>());
    }

    [Test]
    public void GetAsyncSender_And_GetAsyncReceiver_ShouldReturnSameBroker()
    {
        var exchange = new Exchange();
        var sender = exchange.GetAsyncSender<int>();
        var receiver = exchange.GetAsyncReceiver<int>();

        Assert.That(sender, Is.SameAs(receiver));
    }

    [Test]
    public async Task SendAsync_And_ReceiveAsync_ShouldWork()
    {
        var exchange = new Exchange();
        var sender = exchange.GetAsyncSender<string>();
        var receiver = exchange.GetAsyncReceiver<string>();

        var receivedValue = string.Empty;

        await using (receiver.Subscribe(async msg => { receivedValue = msg; await Task.CompletedTask; }))
            await sender.SendAsync("Test Async Message");

        Assert.That(receivedValue, Is.EqualTo("Test Async Message"));
    }
}
