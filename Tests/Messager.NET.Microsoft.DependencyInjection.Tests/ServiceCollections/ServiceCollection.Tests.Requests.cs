using Messager.NET.Microsoft.DependencyInjection.Extensions;
using Messager.NET.Requests;
using Microsoft.Extensions.DependencyInjection;

namespace Messager.NET.Microsoft.DependencyInjection.Tests.ServiceCollections;

public sealed partial class ServiceCollection_Tests
{
    [Test]
    public void AddMessager_ShouldRegisterRequestsFromConfiguredAssemblies()
    {
        var services = new ServiceCollection();

        services.AddMessager(options => options.RequestAssemblies.Add(typeof(TestRequest).Assembly));

        using var provider = services.BuildServiceProvider();
        var request = provider.GetService<IRequest<string>>();

        Assert.That(request, Is.Not.Null);
        Assert.That(request!.Invoke(), Is.EqualTo("pong"));
    }

    [Test]
    public void AddMessager_ShouldNotRegisterRequestsWithoutConfiguredAssemblies()
    {
        var services = new ServiceCollection();

        services.AddMessager();

        using var provider = services.BuildServiceProvider();

        Assert.That(provider.GetService<IRequest<string>>(), Is.Null);
    }
}
