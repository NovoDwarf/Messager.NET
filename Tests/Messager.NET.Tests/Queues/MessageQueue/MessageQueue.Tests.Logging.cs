using System.Diagnostics.CodeAnalysis;
using Messager.NET.Queues;
using Microsoft.Extensions.Logging;
using static Messager.NET.Tests.Helpers.TestHelpers;

namespace Messager.NET.Tests.Queues.MessageQueue;

public sealed partial class MessageQueue_Tests
{
    [Test]
    public async Task HandlerFailure_LogsErrorAndPreservesException()
    {
        var logger = new RecordingLogger();
        var failure = new InvalidOperationException("Handler failed.");
        var queue = new MessageQueue<int>(_ => ValueTask.FromException(failure), logger: logger);

        await queue.EnqueueAsync(1);

        var observed = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await queue.Completion.WaitAsync(DefaultTimeout));
        await Assert.ThrowsAsync<InvalidOperationException>(async () => await queue.DisposeAsync());

        using (Assert.EnterMultipleScope())
        {
            Assert.That(observed, Is.SameAs(failure));
            Assert.That(logger.Count, Is.EqualTo(1));
            Assert.That(logger.EventId.Id, Is.EqualTo(1200));
            Assert.That(logger.Level, Is.EqualTo(LogLevel.Error));
            Assert.That(logger.Exception, Is.SameAs(failure));
        }
    }

    // ReSharper disable once AccessToDisposedClosure
    [Test]
    [SuppressMessage("ReSharper", "MethodSupportsCancellation")]
    public async Task HandlerCancellation_LogsCancellationAndPreservesOutcome()
    {
        var logger = new RecordingLogger();
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();
        var queue = new MessageQueue<int>(_ => ValueTask.FromCanceled(cancellation.Token), logger: logger);

        await queue.EnqueueAsync(1);

        await Assert.ThatAsync(
            async () => await queue.Completion.WaitAsync(DefaultTimeout),
            Throws.InstanceOf<OperationCanceledException>());
        await Assert.ThatAsync(
            async () => await queue.DisposeAsync(),
            Throws.InstanceOf<OperationCanceledException>());

        using (Assert.EnterMultipleScope())
        {
            Assert.That(logger.Count, Is.EqualTo(1));
            Assert.That(logger.EventId.Id, Is.EqualTo(1201));
            Assert.That(logger.Level, Is.EqualTo(LogLevel.Information));
            Assert.That(logger.Exception, Is.InstanceOf<OperationCanceledException>());
        }
    }

    private sealed class RecordingLogger : ILogger<MessageQueue<int>>
    {
        internal int Count { get; private set; }
        internal EventId EventId { get; private set; }
        internal LogLevel Level { get; private set; }
        internal Exception? Exception { get; private set; }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            Count++;
            EventId = eventId;
            Level = logLevel;
            Exception = exception;
        }
    }
}
