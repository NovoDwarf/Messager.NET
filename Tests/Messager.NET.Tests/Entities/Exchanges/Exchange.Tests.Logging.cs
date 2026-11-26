using Messager.NET.Entities;
using Messager.NET.Factories;
using Microsoft.Extensions.Logging;

namespace Messager.NET.Tests.Entities.Exchanges;

public sealed partial class Exchange_Tests
{
    [Test]
    public void KeyedBroker_WithoutLogging_ShouldDeliverWithoutFormattingKey()
    {
        var exchange = new Exchange();
        var key = new UnformattableKey();
        var received = 0;

        using var subscription = exchange.GetKeyedReceiver<UnformattableKey, int>().Subscribe(key, value => received = value);

        exchange.GetKeyedSender<UnformattableKey, int>().Send(key, 42);

        Assert.That(received, Is.EqualTo(42));
    }

    private sealed class UnformattableKey
    {
        public override string ToString() => throw new InvalidOperationException();
    }


    [Test]
    public void Constructor_WithLoggerFactory_ShouldCreateInstance()
    {
        var loggerFactory = LoggerFactory.Create(_ => { });
        var exchange = new Exchange(loggerFactory);

        Assert.That(exchange, Is.Not.Null);
        Assert.That(exchange, Is.InstanceOf<IBrokerFactory>());
        Assert.That(exchange, Is.InstanceOf<IKeyedBrokerFactory>());
    }

}
