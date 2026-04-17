using Autofac;
using Messager.NET.Extensions;
using Messager.NET.Interfaces.Receivers;
using Messager.NET.Interfaces.Senders;
using Microsoft.Extensions.DependencyInjection;

namespace Messager.NET.Tests.Models.Entity.PubSub;

[TestFixture]
public class AsyncPubSubRegistrationsTests
{
	[Test]
	public void AddMessager_ShouldRegisterAsyncPubSubAbstractions()
	{
		var services = new ServiceCollection();

		services.AddMessager();

		using var provider = services.BuildServiceProvider();

		Assert.That(provider.GetService<IAsyncSender<string>>(), Is.Not.Null);
		Assert.That(provider.GetService<IAsyncReceiver<string>>(), Is.Not.Null);
		Assert.That(provider.GetService<IAsyncSender<string, int>>(), Is.Not.Null);
		Assert.That(provider.GetService<IAsyncReceiver<string, int>>(), Is.Not.Null);
	}

	[Test]
	public void AddMessager_ShouldRegisterConfiguredOptionsInServiceCollection()
	{
		var services = new ServiceCollection();

		services.AddMessager(options => options.EnableLogging = false);

		using var provider = services.BuildServiceProvider();
		var options = provider.GetRequiredService<MessagerOptions>();

		Assert.That(options.EnableLogging, Is.False);
	}

	[Test]
	public void AddPubSubRegistrations_ShouldRegisterAsyncPubSubAbstractionsInAutofac()
	{
		var builder = new ContainerBuilder();
		builder.AddMessager();

		using var container = builder.Build();
		using var scope = container.BeginLifetimeScope();

		Assert.That(scope.ResolveOptional<IAsyncSender<string>>(), Is.Not.Null);
		Assert.That(scope.ResolveOptional<IAsyncReceiver<string>>(), Is.Not.Null);
		Assert.That(scope.ResolveOptional<IAsyncSender<string, int>>(), Is.Not.Null);
		Assert.That(scope.ResolveOptional<IAsyncReceiver<string, int>>(), Is.Not.Null);
	}

	[Test]
	public void AddMessager_ShouldRegisterConfiguredOptionsInAutofac()
	{
		var builder = new ContainerBuilder();
		builder.AddMessager(options => options.EnableLogging = false);

		using var container = builder.Build();
		using var scope = container.BeginLifetimeScope();
		var options = scope.Resolve<MessagerOptions>();

		Assert.That(options.EnableLogging, Is.False);
	}
}
