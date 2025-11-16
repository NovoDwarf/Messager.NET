using Messager.NET.Core;
using Messager.NET.Interfaces.Factories;
using Messager.NET.Interfaces.Receivers;
using Messager.NET.Interfaces.Senders;

namespace Messager.NET.Tests.Models.Core;

[TestFixture]
public class ExchangeTests
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
		{
			sender.Send("Test Message");
		}

		Assert.That(receivedValue, Is.EqualTo("Test Message"));
	}

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
		await using (receiver.Subscribe(async msg =>
		{
			receivedValue = msg;
			await Task.CompletedTask;
		}))
		{
			await sender.SendAsync("Test Async Message");
		}

		Assert.That(receivedValue, Is.EqualTo("Test Async Message"));
	}

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
		{
			sender.Send("key1", 42);
		}

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
		await using (receiver.Subscribe("key1", async msg =>
		{
			receivedValue = msg;
			await Task.CompletedTask;
		}))
		{
			await sender.SendAsync("key1", 100);
		}

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

		await using (receiver.Subscribe("key1", async msg =>
		{
			value1 = msg;
			await Task.CompletedTask;
		}))
		await using (receiver.Subscribe("key2", async msg =>
		{
			value2 = msg;
			await Task.CompletedTask;
		}))
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
}
