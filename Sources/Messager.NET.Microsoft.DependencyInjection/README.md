# Messager.NET.Microsoft.DependencyInjection

Microsoft.Extensions.DependencyInjection integration for [Messager.NET](https://www.nuget.org/packages/Messager.NET).

Targets .NET 10 and .NET 11.

```sh
dotnet add package Messager.NET.Microsoft.DependencyInjection
```

```csharp
using Messager.NET.Microsoft.DependencyInjection.Extensions;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
services.AddMessager();
```

Register the services once, then inject the sender and receiver interfaces. This project is under development and is not
production-ready.

See the [repository](https://github.com/NovoDwarf/Messager.NET) for configuration and examples.
