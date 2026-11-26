# Messager.NET.Autofac

Autofac integration for [Messager.NET](https://www.nuget.org/packages/Messager.NET).

Targets .NET 10 and .NET 11.

```sh
dotnet add package Messager.NET.Autofac
```

```csharp
using Autofac;
using Messager.NET.Autofac.Extensions;

var builder = new ContainerBuilder();
builder.AddMessager();
using var container = builder.Build();
```

Resolve sender and receiver interfaces from the container. This project is under development and is not
production-ready.

See the [repository](https://github.com/NovoDwarf/Messager.NET) for configuration and examples.
