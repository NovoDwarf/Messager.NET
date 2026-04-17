using Autofac;
using Messager.NET.Autofac.Extensions;
using Messager.NET.Factories;
using Messager.NET.Receivers;
using Messager.NET.Requests;
using Messager.NET.Senders;

namespace Messager.NET.Tests.Autofac;

[TestFixture]
public class AutofacRegistrationsTests
{
	[Test]
	public void AddMessager_ShouldRegisterPubSubAbstractions()
	{
		var builder = new ContainerBuilder();
		builder.AddMessager();

		using var container = builder.Build();
		using var scope = container.BeginLifetimeScope();

		Assert.That(scope.ResolveOptional<ISender<string>>(), Is.Not.Null);
		Assert.That(scope.ResolveOptional<IReceiver<string>>(), Is.Not.Null);
		Assert.That(scope.ResolveOptional<IAsyncSender<string>>(), Is.Not.Null);
		Assert.That(scope.ResolveOptional<IAsyncReceiver<string>>(), Is.Not.Null);
		Assert.That(scope.ResolveOptional<ISender<string, int>>(), Is.Not.Null);
		Assert.That(scope.ResolveOptional<IReceiver<string, int>>(), Is.Not.Null);
		Assert.That(scope.ResolveOptional<IAsyncSender<string, int>>(), Is.Not.Null);
		Assert.That(scope.ResolveOptional<IAsyncReceiver<string, int>>(), Is.Not.Null);
	}

	[Test]
	public void AddMessager_ShouldRegisterConfiguredOptions()
	{
		var builder = new ContainerBuilder();
		builder.AddMessager(options => options.EnableLogging = false);

		using var container = builder.Build();
		using var scope = container.BeginLifetimeScope();
		var options = scope.Resolve<MessagerOptions>();

		Assert.That(options.EnableLogging, Is.False);
	}

	[Test]
	public void AddMessager_ShouldRegisterRequestsFromConfiguredAssemblies()
	{
		var builder = new ContainerBuilder();
		builder.AddMessager(options => options.RequestAssemblies.Add(typeof(TestAutofacRequest).Assembly));

		using var container = builder.Build();
		using var scope = container.BeginLifetimeScope();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(scope.ResolveOptional<IRequestFactory>(), Is.Not.Null);
            Assert.That(scope.ResolveOptional<IRequest<string>>(), Is.Not.Null);
            Assert.That(scope.Resolve<IRequest<string>>().Invoke(), Is.EqualTo("pong"));
        }
    }
}

internal sealed class TestAutofacRequest : IRequest<string>
{
	public string Invoke() => "pong";
}
