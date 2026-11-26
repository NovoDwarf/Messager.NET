using System.Diagnostics.CodeAnalysis;
using System.Threading.Channels;
using Messager.NET.Queues;
using static Messager.NET.Tests.Helpers.TestHelpers;

namespace Messager.NET.Tests.Queues.MessageQueue;

public sealed partial class MessageQueue_Tests
{
    [TestCase(false)]
    [TestCase(true)]
    public async Task HandlerFailure_FaultsCompletionAndDisposeWithoutLogger(bool asynchronousFailure)
    {
        var failure = new InvalidOperationException("Handler failed.");
        var queue = new MessageQueue<int>(_ => asynchronousFailure
            ? ValueTask.FromException(failure)
            : throw failure);

        await queue.EnqueueAsync(1);
        var observed = await Assert.ThrowsAsync<InvalidOperationException>(async () => await queue.Completion.WaitAsync(DefaultTimeout));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(observed, Is.SameAs(failure));
            Assert.That(queue.TryEnqueue(2), Is.False);
        }

        await Assert.ThrowsAsync<ChannelClosedException>(async () => await queue.EnqueueAsync(3));
        var disposed = await Assert.ThrowsAsync<InvalidOperationException>(async () => await queue.DisposeAsync());
        Assert.That(disposed, Is.SameAs(failure));
    }

    [Test]
    public async Task HandlerFailure_RejectsWaitingWriteAndDiscardsBufferedMessages()
    {
        var entered = NewSignal();
        var release = NewSignal();
        var received = new List<int>();

        var queue = new MessageQueue<int>(async message =>
        {
            received.Add(message);
            entered.SetResult();
            await release.Task;
            throw new InvalidOperationException("Handler failed.");
        }, capacity: 1);

        try
        {
            await queue.EnqueueAsync(1);
            await entered.Task.WaitAsync(DefaultTimeout);
            await queue.EnqueueAsync(2);

            var pending = queue.EnqueueAsync(3).AsTask();

            release.SetResult();

            await Assert.ThrowsAsync<InvalidOperationException>(async () => await queue.Completion.WaitAsync(DefaultTimeout));
            await Assert.ThrowsAsync<ChannelClosedException>(async () => await pending.WaitAsync(DefaultTimeout));

            Assert.That(received, Is.EqualTo([1]));
        }
        finally
        {
            release.TrySetResult();
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await queue.DisposeAsync());
        }
    }

    // ReSharper disable once AccessToDisposedClosure
    [Test]
    [SuppressMessage("ReSharper", "MethodSupportsCancellation")]
    public async Task HandlerCancellation_CancelsCompletionAndClosesAdmission()
    {
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();
        var queue = new MessageQueue<int>(_ => ValueTask.FromCanceled(cancellation.Token));

        await queue.EnqueueAsync(1);
        await Assert.ThatAsync(async () => await queue.Completion.WaitAsync(DefaultTimeout), Throws.InstanceOf<OperationCanceledException>());

        using (Assert.EnterMultipleScope())
        {
            Assert.That(queue.Completion.IsCanceled, Is.True);
            Assert.That(queue.TryEnqueue(2), Is.False);
        }

        await Assert.ThatAsync(async () => await queue.EnqueueAsync(3), Throws.InstanceOf<OperationCanceledException>());
        await Assert.ThatAsync(async () => await queue.DisposeAsync(), Throws.InstanceOf<OperationCanceledException>());
    }
}
