using Messager.NET.Entities;

namespace Messager.NET.Tests.Models.Brokers;

[TestFixture]
public sealed class SubscriptionTests
{
    [TestCase(false)]
    [TestCase(true)]
    public void Dispose_DuplicateHandler_RemovesOnlyItsOwnSubscription(bool keyed)
    {
        var exchange = new Exchange();
        var calls = 0;
       
        Action<int> handler = _ => calls++;
       
        using var first = keyed
            ? exchange.GetKeyedReceiver<string, int>().Subscribe("key", handler)
            : exchange.GetReceiver<int>().Subscribe(handler);
        
        using var second = keyed
            ? exchange.GetKeyedReceiver<string, int>().Subscribe("key", handler)
            : exchange.GetReceiver<int>().Subscribe(handler);

        first.Dispose();

        if (keyed)
        {
            exchange.GetKeyedSender<string, int>().Send("key", 1);
        }
        else
        {
            exchange.GetSender<int>().Send(1);
        }

        Assert.That(calls, Is.EqualTo(1));
    }

    [TestCase(false)]
    [TestCase(true)]
    public void Send_MulticastHandler_InvokesInOrderAndDisposesAsOneSubscription(bool keyed)
    {
        var exchange = new Exchange();
        var calls = new List<int>();
       
        Action<int> first = value => calls.Add(value);
        Action<int> second = value => calls.Add(value + 1);
       
        using var subscription = keyed
            ? exchange.GetKeyedReceiver<string, int>().Subscribe("key", first + second + first)
            : exchange.GetReceiver<int>().Subscribe(first + second + first);

        void Send()
        {
            if (keyed)
            {
                exchange.GetKeyedSender<string, int>().Send("key", 10);
            }
            else
            {
                exchange.GetSender<int>().Send(10);
            }
        }

        Send();
        subscription.Dispose();
        Send();

        Assert.That(calls, Is.EqualTo([10, 11, 10]));
    }

    [TestCase(false)]
    [TestCase(true)]
    public void Send_ThrowingMulticastHandler_ContinuesWithNextSubscription(bool keyed)
    {
        var exchange = new Exchange();
        var calls = new List<int>();
       
        Action<int> throwing = _ => throw new InvalidOperationException();
        Action<int> skipped = _ => calls.Add(1);
        Action<int> next = _ => calls.Add(2);
       
        using var first = keyed
            ? exchange.GetKeyedReceiver<string, int>().Subscribe("key", throwing + skipped)
            : exchange.GetReceiver<int>().Subscribe(throwing + skipped);
       
        using var second = keyed
            ? exchange.GetKeyedReceiver<string, int>().Subscribe("key", next)
            : exchange.GetReceiver<int>().Subscribe(next);

        if (keyed)
        {
            exchange.GetKeyedSender<string, int>().Send("key", 1);
        }
        else
        {
            exchange.GetSender<int>().Send(1);
        }

        Assert.That(calls, Is.EqualTo([2]));
    }

    [Test]
    public async Task KeyedSend_HandlerCanWaitForSubscriptionOnAnotherThread()
    {
        var exchange = new Exchange();
        var receiver = exchange.GetKeyedReceiver<string, int>();
        var sender = exchange.GetKeyedSender<string, int>();
        var completedInsideHandler = false;
        
        Task? subscribeTask = null;
      
        using var subscription = receiver.Subscribe("key", _ =>
        {
            subscribeTask = Task.Run(() =>
            {
                using var nested = receiver.Subscribe("other", _ => { });
            });
            completedInsideHandler = subscribeTask.Wait(TimeSpan.FromSeconds(5));
        });

        await Task.Run(() => sender.Send("key", 1));
        await subscribeTask!;

        Assert.That(completedInsideHandler, Is.True);
    }

    [Test]
    public void KeyedSend_ResubscriptionDuringCallback_SurvivesNextSend()
    {
        var exchange = new Exchange();
        var receiver = exchange.GetKeyedReceiver<string, int>();
        var sender = exchange.GetKeyedSender<string, int>();
        
        IDisposable? original = null;
        IDisposable? replacement = null;
        
        var calls = 0;
       
        original = receiver.Subscribe("key", _ =>
        {
            original!.Dispose();
            replacement = receiver.Subscribe("key", _ => calls++);
        });

        try
        {
            sender.Send("key", 1);
            Assert.That(calls, Is.Zero);
            sender.Send("key", 2);
            Assert.That(calls, Is.EqualTo(1));
        }
        finally
        {
            original.Dispose();
            replacement?.Dispose();
        }
    }
}
