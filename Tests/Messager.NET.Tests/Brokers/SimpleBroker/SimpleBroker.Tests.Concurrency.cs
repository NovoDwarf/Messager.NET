using Messager.NET.Models.Brokers;
using static Messager.NET.Tests.Helpers.TestHelpers;

namespace Messager.NET.Tests.Brokers.SimpleBroker;

[TestFixture]
public sealed partial class SimpleBroker_Tests
{
    [Test]
    public async Task Send_WithConcurrentSubscriptionsAndDeadHandlerCleanup_ShouldNotThrow()
    {
        var broker = new SimpleBroker<int>();

        for (var i = 0; i < 32; i++)
            CreateCollectableSubscription(broker);

        CollectGarbage();

        var tasks = Enumerable.Range(0, Environment.ProcessorCount)
            .Select(_ => Task.Run(() =>
            {
                for (var i = 0; i < 200; i++)
                {
                    using var subscription = broker.Subscribe(_ => { });
                    broker.Send(i);
                }
            }));

        await Assert.DoesNotThrowAsync(async () => await Task.WhenAll(tasks));
    }

    private static void CreateCollectableSubscription(SimpleBroker<int> broker)
    {
        var subscriber = new TemporarySubscriber();
        broker.Subscribe(subscriber.Handle);
    }


    private sealed class TemporarySubscriber
    {
        public void Handle(int _) { }
    }
}
