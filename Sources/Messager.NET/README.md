# Messager.NET

A typed messaging library with synchronous and asynchronous publish/subscribe, keyed messages and an in-memory
background queue.

Targets .NET 10 and .NET 11.

```sh
dotnet add package Messager.NET
```

```csharp
using Messager.NET.Entities;

var exchange = new Exchange();
using var subscription = exchange.GetReceiver<string>()
    .Subscribe(message => Console.WriteLine(message));

exchange.GetSender<string>().Send("Hello");
```

Dispose the subscription when it is no longer needed. For dependency injection, use the Microsoft or Autofac integration
packages.

This project is under development and is not production-ready. See
the [repository](https://github.com/NovoDwarf/Messager.NET) for usage and subscription behavior.
