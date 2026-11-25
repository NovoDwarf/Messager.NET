using Messager.NET.Queues;
using Microsoft.Extensions.Logging;

namespace Messager.NET.Tests.Queues;

[TestFixture]
public sealed class MessageQueueLoggingTests
{
    [TestCase(false)]
    [TestCase(true)]
    public async Task HandlerStops_LogsTheReasonAndPreservesCompletionOutcome(bool canceled)
    {
        var logger = new RecordingLogger();
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var failure = new InvalidOperationException("Handler failed.");
        var queue = new MessageQueue<int>(
            _ => canceled ? ValueTask.FromCanceled(cancellation.Token) : ValueTask.FromException(failure),
            logger: logger);

        await queue.EnqueueAsync(1);

        if (canceled)
        {
            await Assert.ThatAsync(async () => await queue.Completion.WaitAsync(TimeSpan.FromSeconds(10)),
                Throws.InstanceOf<OperationCanceledException>());
            await Assert.ThatAsync(async () => await queue.DisposeAsync(),
                Throws.InstanceOf<OperationCanceledException>());
        }
        else
        {
            var observed = await Assert.ThrowsAsync<InvalidOperationException>(
                async () => await queue.Completion.WaitAsync(TimeSpan.FromSeconds(10)));
            Assert.That(observed, Is.SameAs(failure));
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await queue.DisposeAsync());
        }

        using (Assert.EnterMultipleScope())
        {
            Assert.That(logger.Count, Is.EqualTo(1));
            Assert.That(logger.EventId.Id, Is.EqualTo(canceled ? 1201 : 1200));
            Assert.That(logger.Level, Is.EqualTo(canceled ? LogLevel.Information : LogLevel.Error));
            Assert.That(logger.Exception, canceled
                ? Is.InstanceOf<OperationCanceledException>()
                : Is.SameAs(failure));
        }
    }

    private sealed class RecordingLogger : ILogger<MessageQueue<int>>
    {
        internal int Count { get; private set; }
        internal EventId EventId { get; private set; }
        internal LogLevel Level { get; private set; }
        internal Exception? Exception { get; private set; }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            Count++;
            EventId = eventId;
            Level = logLevel;
            Exception = exception;
        }
    }
}
