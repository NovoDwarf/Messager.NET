using Messager.NET.Enums;
using Messager.NET.Microsoft.DependencyInjection.Extensions;
using Messager.NET.Receivers;
using Microsoft.Extensions.DependencyInjection;

namespace Messager.NET.Microsoft.DependencyInjection.Tests.ServiceCollections;

public sealed partial class ServiceCollection_Tests
{
    [TestCase(false, false)]
    [TestCase(false, true)]
    [TestCase(true, false)]
    [TestCase(true, true)]
    public async Task DependencyInjection_ReceiversForwardReferenceMode(bool asynchronous, bool keyed)
    {
        var services = new ServiceCollection();
        services.AddMessager();

        await using var provider = services.BuildServiceProvider();

        var target = new Subscriber(new Counter());
        object handle;

        if (asynchronous)
        {
            handle = keyed
                ? provider.GetRequiredService<IAsyncReceiver<string, int>>().Subscribe("key", target.HandleAsync, SubscriptionReference.Weak)
                : provider.GetRequiredService<IAsyncReceiver<int>>().Subscribe(target.HandleAsync, SubscriptionReference.Weak);
        }
        else
        {
            handle = keyed
                ? provider.GetRequiredService<IReceiver<string, int>>().Subscribe("key", target.Handle, SubscriptionReference.Strong)
                : provider.GetRequiredService<IReceiver<int>>().Subscribe(target.Handle, SubscriptionReference.Strong);
        }

        await DisposeAsync(handle);
    }

    private static async ValueTask DisposeAsync(object handle)
    {
        if (handle is IAsyncDisposable asynchronous)
        {
            await asynchronous.DisposeAsync();
        }
        else
        {
            ((IDisposable)handle).Dispose();
        }
    }

    private sealed class Counter
    {
        internal int Calls { get; set; }
    }

    private sealed class Subscriber(Counter counter)
    {
        internal void Handle(int value)
        {
            counter.Calls++;
        }

        internal ValueTask HandleAsync(int value)
        {
            counter.Calls++;
            return ValueTask.CompletedTask;
        }
    }
}
