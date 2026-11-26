using System.Runtime.CompilerServices;
using Messager.NET.Queues;

namespace Messager.NET.Tests.Queues.MessageQueue;

public sealed partial class MessageQueue_Tests
{

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static (MessageQueue<int> Queue, WeakReference Target) CreateQueueWithCollectableHandler()
    {
        var handler = new QueueHandler();
        return (new MessageQueue<int>(handler.HandleAsync), new WeakReference(handler));
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference EnqueueCollectablePayload(MessageQueue<object> queue)
    {
        var message = new object();
        Assert.That(queue.TryEnqueue(message), Is.True);
        return new WeakReference(message);
    }


    private sealed class QueueHandler
    {
        internal ValueTask HandleAsync(int message) => ValueTask.CompletedTask;
    }
}
