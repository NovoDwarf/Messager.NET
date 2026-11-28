# Changelog

## 2.1.0

### Messager.NET.PureDI

- Add a Pure.DI integration for synchronous, asynchronous and keyed pub/sub. Request handlers must be registered
  explicitly.

### All packages

- Include portable debugging symbols and XML documentation in the NuGet packages.
- Add release notes metadata and expand public API documentation.

### Messager.NET.Core

- **Breaking change:** `Subscribe` is now required on all `IReceiver` and `IAsyncReceiver` interfaces. Custom
  implementations relying on the previous default implementation must implement the overload accepting
  `SubscriptionReference` and be rebuilt.
- **Breaking change:** `SubscriptionReference` moved from `Messager.NET.Receivers` to `Messager.NET.Enums`; update
  imports and rebuild code that uses it.

### Messager.NET, Messager.NET.Autofac and Messager.NET.Microsoft.DependencyInjection

- Add JetBrains annotations and the corresponding package dependency for consumer-side analysis.

## 2.0.0

### Messager.NET

- Remove collected weak subscriptions from brokers during message dispatch.
- Include package metadata and a package README.

### Messager.NET.Core

- Include package metadata and a package README.

### Messager.NET.Microsoft.DependencyInjection

- Include package metadata and a package README.

### Messager.NET.Autofac

- Include package metadata and a package README.
