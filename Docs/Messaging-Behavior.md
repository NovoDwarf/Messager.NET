# Subscription and queue behavior

## Subscription behavior

- Every `Subscribe` call creates an independent registration, including repeated registrations of the same delegate.
  Disposing its handle removes only that registration.
- Subscriptions use strong references by default. Pass `SubscriptionReference.Weak` to allow callback targets to be
  collected while the subscription remains registered.
- Multicast delegates run in invocation-list order. Async brokers await each callback before invoking the next one.
- An exception stops the remaining callbacks in that multicast registration. The broker logs the exception when logging
  is enabled and continues with the next registration.
- Sends use a snapshot of registrations. Subscription changes made during a callback apply to subsequent sends;
  disposing a handle does not cancel callbacks already included in a snapshot.

## Explicit strong and weak subscriptions

All synchronous and asynchronous receivers, including keyed receivers and DI wrappers, accept an explicit
`SubscriptionReference.Strong` or `SubscriptionReference.Weak` argument on `Subscribe`.

- `Strong` keeps the callback targets alive while registered, even if the subscription handle is discarded.
- `Weak` does not keep callback targets alive, even if the subscription handle is retained. Keep an independent strong
  reference to the subscriber for as long as it must receive messages. For capturing lambdas, the target can be a
  compiler-generated closure, which also needs an independent owner.
- Static callbacks remain available in either mode. A multicast weak subscription skips collected targets and continues
  to invoke surviving callbacks in order. Fully dead registrations are removed on a subsequent send; for keyed
  subscriptions, cleanup occurs when that key is sent to. A key that is never sent to again remains registered until its
  subscription handles are disposed.
- Removing a subscription does not interrupt invocations already included in a send snapshot. An asynchronous callback
  can retain its target until its operation completes.

## Background queues

`MessageQueue<TMessage>` is a separate, explicitly owned queue backed by `Channel<TMessage>`. It starts one background
worker at construction. It does not change `Exchange`, `Send`, or `SendAsync`.

```csharp
using Messager.NET.Queues;

await using var queue = new MessageQueue<string>(
    async message =>
    {
        await Task.Delay(25);
        Console.WriteLine(message);
    },
    capacity: 32);

IMessageQueue<string> producer = queue;
await producer.EnqueueAsync("Build document preview");
await queue.CompleteAsync();
```

- `EnqueueAsync` waits for buffer capacity and confirms admission only. It does not wait for processing. Its
  cancellation token can cancel a waiting write but does not retract a message already admitted.
- `TryEnqueue` immediately returns false if the buffer is full or admission has closed.
- One worker processes messages in admission order. Concurrent producers are supported; their relative order depends on
  which write is admitted first. Capacity excludes the message currently being processed and payloads held by producers
  waiting to write.
- `Complete` closes admission immediately. `Completion` completes after the accepted messages have been handled.
  `CompleteAsync` closes admission and waits for that outcome. Canceling its wait leaves processing running.
- `DisposeAsync` also closes admission and drains the queue. Repeated and concurrent calls observe the same worker
  outcome. Dispose owned queues asynchronously; complete them before disposing handler dependencies.
- The first handler exception stops the worker, rejects waiting writers, and discards remaining buffered messages. The
  original exception is propagated by `Completion`, `CompleteAsync`, and `DisposeAsync`, even without a logger. A
  handler cancellation cancels `Completion`. An optional `ILogger<MessageQueue<TMessage>>` records the stop. Rejected
  writes throw `ChannelClosedException`, or `OperationCanceledException` when the handler was canceled.
- A multicast handler runs in invocation-list order and every `ValueTask` is awaited. A failure prevents later callbacks
  and later messages from being processed.
- The queue strongly retains its handler until processing ends. It does not serialize or copy messages; prefer immutable
  payloads. A queued message will not survive process termination. This version has no retries or persistence and does
  not provide delivery to every pub/sub subscriber.
- Do not await completion or disposal from the queue's own handler. A handler that awaits admission back into the same
  full queue also prevents progress. A handler that never finishes prevents graceful shutdown.

Applications can inject the producer-only `IMessageQueue<TMessage>` into services while keeping the concrete queue and
its lifecycle under explicit ownership. A complete runnable example is
in [Examples/Messager.NET.Queued](../Examples/Messager.NET.Queued):

```text
dotnet run --project Examples/Messager.NET.Queued -f net10.0 -c Release
```

