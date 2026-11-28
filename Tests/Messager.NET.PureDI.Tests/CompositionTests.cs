using Messager.NET.PureDI;
using Messager.NET.PureDI.Models;

namespace Messager.NET.PureDI.Tests;

public sealed class CompositionTests
{
	[Test]
	public void Messaging_UsesOneExchangeAcrossRootsAndIsolatesCompositions()
	{
		var composition = new Composition();
		var otherComposition = new Composition();
		var received = new List<string>();
		var subscription = composition.Messaging.Receiver.Subscribe(received.Add);

		composition.Messaging.Sender.Send("same composition");
		otherComposition.Messaging.Sender.Send("other composition");
		subscription.Dispose();
		composition.Messaging.Sender.Send("removed");

		Assert.That(composition.BrokerFactory, Is.SameAs(composition.KeyedBrokerFactory));
		Assert.That(composition.BrokerFactory, Is.Not.SameAs(otherComposition.BrokerFactory));
		Assert.That(received, Is.EqualTo(["same composition"]));
	}

	[Test]
	public void GenericBindings_SupportDifferentEventTypes()
	{
		var composition = new Composition();
		var received = new List<int>();
		using var subscription = composition.NumberReceiver.Subscribe(received.Add);

		composition.NumberSender.Send(42);

		Assert.That(received, Is.EqualTo([42]));
	}

	[Test]
	public async Task Messaging_DeliversKeyedAndAsyncMessagesAndRemovesSubscriptions()
	{
		var messaging = new Composition().Messaging;
		var received = new List<string>();

		using var keyed = messaging.KeyedReceiver.Subscribe("key", received.Add);

		var asyncSubscription = messaging.AsyncReceiver.Subscribe(message =>
		{
			received.Add(message);
			return ValueTask.CompletedTask;
		});
		var asyncKeyed = messaging.AsyncKeyedReceiver.Subscribe("key", message =>
		{
			received.Add(message);
			return ValueTask.CompletedTask;
		});

		messaging.KeyedSender.Send("other", "ignored");
		messaging.KeyedSender.Send("key", "keyed");

		await messaging.AsyncSender.SendAsync("async");
		await messaging.AsyncKeyedSender.SendAsync("other", "ignored");
		await messaging.AsyncKeyedSender.SendAsync("key", "async keyed");
		await asyncSubscription.DisposeAsync();
		await asyncKeyed.DisposeAsync();
		await messaging.AsyncSender.SendAsync("removed");
		await messaging.AsyncKeyedSender.SendAsync("key", "removed");

		Assert.That(received, Is.EqualTo(["keyed", "async", "async keyed"]));
	}

	[Test]
	public void Request_ResolvesExplicitlyRegisteredHandler()
	{
		var composition = new Composition();

		var result = composition.StringLength.Invoke("Pure.DI");

		Assert.That(result, Is.EqualTo(7));
	}
}