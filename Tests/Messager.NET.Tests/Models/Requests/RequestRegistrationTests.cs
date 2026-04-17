using Autofac;
using Messager.NET.Extensions;
using Messager.NET.Interfaces.Requests;
using Microsoft.Extensions.DependencyInjection;

namespace Messager.NET.Tests.Models.Requests;

[TestFixture]
public class RequestRegistrationTests
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

	[Test]
	public void AutofacAddMessager_ShouldRegisterRequestsFromConfiguredAssemblies()
	{
		var builder = new ContainerBuilder();
		builder.AddMessager(options => options.RequestAssemblies.Add(typeof(TestRequest).Assembly));

		using var container = builder.Build();
		using var scope = container.BeginLifetimeScope();
		var request = scope.ResolveOptional<IRequest<string>>();

		Assert.That(request, Is.Not.Null);
		Assert.That(request!.Invoke(), Is.EqualTo("pong"));
	}
}

internal sealed class TestRequest : IRequest<string>
{
	public string Invoke() => "pong";
}
