# Messager.NET.PureDI

Compile-time Pure.DI bindings for Messager.NET pub/sub. Install `Messager.NET.PureDI` and `Pure.DI` 2.5.11 in the
project containing your composition.

Add the setup to your composition:

```csharp
DI.Setup(nameof(Composition))
    .DependsOn("Messager.NET.PureDI.MessagerBindings")
    // Add application bindings and roots here.
    ;
```

The setup provides synchronous, asynchronous, keyed and non-keyed senders and receivers. Both broker factory interfaces
share one `Exchange` per composition; separate compositions are isolated. Senders and receivers are transient. The setup
constructs `Exchange` without logging; configuring a logger or using `Exchange.Default` is not covered by this
integration.

This integration does not register `IRequestFactory` or `IAsyncRequestFactory`, discover request handlers in assemblies,
or use `MessagerOptions`. Bind request handlers explicitly in your Pure.DI composition and inject them directly. Pure.DI
generic roots cannot be resolved using its generated `Resolve` methods. `ResolveAll`, request-factory semantics and
scoped request handlers are not provided by this package.

The package imports its setup source into consuming projects. Each consuming project must reference the Pure.DI
generator directly.
