using Messager.NET.DependencyInjection;
using Messager.NET.DependencyInjection.Extensions;
using Messager.NET.Extensions;
using Messager.NET.Receivers;
using Messager.NET.Senders;
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
}
