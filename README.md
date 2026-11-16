# Messager.NET

A messaging library for .NET with typed pub/sub, request contracts, and an in-memory background queue.

> [!WARNING]
> This project was created for educational purposes and is still under development. It may contain bugs, incomplete features, and architectural flaws. It is not production-ready.

The projects target .NET 10 and .NET 11. Pub/sub supports synchronous and asynchronous handlers, with optional keys. `Exchange` creates brokers as they are needed and provides senders and receivers for each event type.

## Projects

- `Messager.NET.Core` — interfaces for messaging, requests, and queues.
- `Messager.NET` — pub/sub implementation and background queue.
- `Messager.NET.Microsoft.DependencyInjection` — integration with Microsoft.Extensions.DependencyInjection.
- `Messager.NET.Autofac` — Autofac integration.

Reference the projects you need. The DI integrations reference the runtime project, which in turn references Core.

## Pub/sub

Create an `Exchange`, subscribe a handler, and send an event. Keep the subscription handle and dispose it when the handler is no longer needed.

```csharp
using Messager.NET.Entities;

var exchange = new Exchange();
var receiver = exchange.GetReceiver<string>();
var sender = exchange.GetSender<string>();

using var subscription = receiver.Subscribe(message => Console.WriteLine(message));
sender.Send("Hello");
```

For keyed messages, use `GetKeyedReceiver<TKey, TEvent>()` and `GetKeyedSender<TKey, TEvent>()`. Only subscribers for the matching key receive the event.

```csharp
var receiver = exchange.GetKeyedReceiver<int, string>();
var sender = exchange.GetKeyedSender<int, string>();

using var subscription = receiver.Subscribe(1, message => Console.WriteLine(message));
sender.Send(1, "Hello");
```

Async senders and receivers are separate from synchronous ones. Async handlers return `ValueTask`, and the subscription handle implements `IAsyncDisposable`.

```csharp
var receiver = exchange.GetAsyncReceiver<string>();
var sender = exchange.GetAsyncSender<string>();

await using var subscription = receiver.Subscribe(async message =>
{
    await Task.Delay(25);
    Console.WriteLine(message);
});

await sender.SendAsync("Hello");
```

Keyed async variants are available through `GetAsyncKeyedReceiver<TKey, TEvent>()` and `GetAsyncKeyedSender<TKey, TEvent>()`.

## Requests

Request interfaces represent a handler that returns a result. They can be used directly or registered through either DI integration. Their generic arguments put the output before the input: `IRequest<TOut>`, `IRequest<TOut, TIn>`, and `IRequest<TKey, TOut, TIn>`. The same forms are available for `IAsyncRequest`, whose `InvokeAsync` methods return `ValueTask<TOut>`.

To register request implementations from an assembly:

```csharp
services.AddMessager(options => options.RequestAssemblies.Add(typeof(MyRequest).Assembly));
```

Request implementations are registered by their request interfaces. Inject an interface directly, or use `IRequestFactory` / `IAsyncRequestFactory` to resolve one or all matching handlers. Keyed request interfaces pass the key to the handler; they do not use the pub/sub broker for routing.

## Subscription and queue behavior

### Subscription behavior

- Every `Subscribe` call creates an independent registration, including repeated registrations of the same delegate. Disposing its handle removes only that registration.
- Synchronous brokers hold weak references to callback targets. Retaining the subscription handle keeps its callbacks alive until disposal. A multicast subscription remains active while any callback target is alive; callbacks whose targets have been collected are skipped.
- Multicast delegates run in invocation-list order. Async brokers await each callback before invoking the next one.
- An exception stops the remaining callbacks in that multicast registration. The broker logs the exception when logging is enabled and continues with the next registration.
- Sends use a snapshot of registrations. Subscription changes made during a callback apply to subsequent sends; disposing a handle does not cancel callbacks already included in a snapshot.

### Explicit strong and weak subscriptions

All synchronous and asynchronous receivers, including keyed receivers and DI wrappers, accept an explicit `SubscriptionReference.Strong` or `SubscriptionReference.Weak` argument on `Subscribe`.

- `Strong` keeps the callback targets alive while registered, even if the subscription handle is discarded.
- `Weak` does not keep callback targets alive, even if the subscription handle is retained. Keep an independent strong reference to the subscriber for as long as it must receive messages. For capturing lambdas, the target can be a compiler-generated closure, which also needs an independent owner.
- Static callbacks remain available in either mode. A multicast weak subscription skips collected targets and  continues to invoke surviving callbacks in order. Dead registrations are cleaned up on subsequent sends.
- Removing a subscription does not interrupt invocations already included in a send snapshot. An asynchronous callback can retain its target until its operation completes.

The original `Subscribe(handler)` and `Subscribe(key, handler)` overloads preserve their previous behavior: synchronous subscriptions use weak broker references with a handle that retains the callbacks; asynchronous subscriptions use strong references. Use the explicit overloads for unambiguous ownership in new code.

Existing third-party receiver implementations can continue to implement only the original overloads. The new overloads throw `NotSupportedException` unless the implementation supports explicit reference modes.

### Background queues

`MessageQueue<TMessage>` is a separate, explicitly owned queue backed by `Channel<TMessage>`. It starts one background worker at construction. It does not change `Exchange`, `Send`, or `SendAsync`.

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

- `EnqueueAsync` waits for buffer capacity and confirms admission only. It does not wait for processing. Its cancellation token can cancel a waiting write but does not retract a message already admitted.
- `TryEnqueue` immediately returns false if the buffer is full or admission has closed.
- One worker processes messages in admission order. Concurrent producers are supported; their relative order depends on which write is admitted first. Capacity excludes the message currently being processed and payloads held by producers waiting to write.
- `Complete` closes admission immediately. `Completion` completes after the accepted messages have been handled. `CompleteAsync` closes admission and waits for that outcome. Canceling its wait leaves processing running.
- `DisposeAsync` also closes admission and drains the queue. Repeated and concurrent calls observe the same  worker outcome. Dispose owned queues asynchronously; complete them before disposing handler dependencies.
- The first handler exception stops the worker, rejects waiting writers, and discards remaining buffered messages. The original exception is propagated by `Completion`, `CompleteAsync`, and `DisposeAsync`, even without a logger. A handler cancellation cancels `Completion`. An optional `ILogger<MessageQueue<TMessage>>` records the stop. Rejected writes throw `ChannelClosedException`, or `OperationCanceledException` when the handler was canceled.
- A multicast handler runs in invocation-list order and every `ValueTask` is awaited. A failure prevents later callbacks and later messages from being processed.
- The queue strongly retains its handler until processing ends. It does not serialize or copy messages; prefer immutable payloads. A queued message will not survive process termination. This version has no retries or persistence and does not provide delivery to every pub/sub subscriber.
- Do not await completion or disposal from the queue's own handler. A handler that awaits admission back into the same full queue also prevents progress. A handler that never finishes prevents graceful shutdown.

Applications can inject the producer-only `IMessageQueue<TMessage>` into services while keeping the concrete queue and its lifecycle under explicit ownership. A complete runnable example is in `Examples/Messager.NET.Queued`:

```text
dotnet run --project Examples/Messager.NET.Queued -f net10.0 -c Release
```

## Dependency injection

With Microsoft.Extensions.DependencyInjection, call `AddMessager()` and inject the sender or receiver interface into your service:

```csharp
using Messager.NET.Microsoft.DependencyInjection.Extensions;

services.AddMessager();
```

Autofac provides the same registration method through `Messager.NET.Autofac.Extensions`:

```csharp
builder.AddMessager();
```

Each container gets a shared `Exchange`. Sender and receiver interfaces are registered for synchronous, asynchronous, keyed, and unkeyed messages. You can pass an `ILoggerFactory` directly to `Exchange`, or register an `ILoggerFactory` in the container. DI logging is enabled by default and can be disabled with `options.EnableLogging`.

## Examples

The [Examples](Examples) directory contains synchronous, asynchronous, keyed, request, and queue examples. For example:

```text
dotnet run --project Examples/Messager.NET.Simple -f net10.0
```

## Thanks

Inspired by [MessagePipe](https://github.com/Cysharp/MessagePipe) by [Cysharp](https://github.com/Cysharp).

## License

[MIT](LICENSE)
