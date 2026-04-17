using Messager.NET.Models.Brokers;

namespace Messager.NET.Tests.Models.Brokers;

[TestFixture]
public class SimpleBrokerTestsThreadSafety
{
	[Test]
	public async Task Send_WithConcurrentSubscriptionsAndDeadHandlerCleanup_ShouldNotThrow()
	{
		var broker = new SimpleBroker<int>();

		for (var i = 0; i < 32; i++)
			CreateCollectableSubscription(broker);

		CollectSubscriptions();

		var tasks = Enumerable.Range(0, Environment.ProcessorCount)
			.Select(_ => Task.Run(() =>
			{
				for (var i = 0; i < 200; i++)
				{
					using var subscription = broker.Subscribe(_ => { });
					broker.Send(i);
				}
			}));

		Assert.DoesNotThrowAsync(async () => await Task.WhenAll(tasks));
	}

	private static void CreateCollectableSubscription(SimpleBroker<int> broker)
	{
		var subscriber = new TemporarySubscriber();
		broker.Subscribe(subscriber.Handle);
	}

	private static void CollectSubscriptions()
	{
		GC.Collect();
		GC.WaitForPendingFinalizers();
		GC.Collect();
	}

	private sealed class TemporarySubscriber
	{
		public void Handle(int _)
		{
		}
	}
}
