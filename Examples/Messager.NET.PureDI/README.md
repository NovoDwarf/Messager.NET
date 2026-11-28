# Pure.DI example

This example uses Pure.DI to compose Messager.NET without a runtime DI container.
Run it with:

```shell
dotnet run --project Examples/Messager.NET.PureDI -f net10.0
```

`Composition` uses the integration setup, which binds the same singleton `Exchange` to both broker factory interfaces.
Senders and receivers created by one composition therefore share subscriptions;
separate compositions remain isolated. The example registers generic synchronous,
asynchronous and keyed sender/receiver pairs, and an explicitly registered request
handler.

The existing `IRequestFactory` and `IAsyncRequestFactory` interfaces are not registered
by this example. They rely on runtime resolution, while this composition exposes its
request handler as a generated, strongly typed root. Pure.DI does not resolve generic
composition roots through its generated `Resolve` methods, so a general-purpose
adapter requires further investigation, including `ResolveAll` and scoped handlers.
Request handler assembly scanning from the existing DI integrations is not performed.

The example links the setup source for local development. For NuGet installation, see
the [package README](../../Sources/Messager.NET.PureDI/README.md).
