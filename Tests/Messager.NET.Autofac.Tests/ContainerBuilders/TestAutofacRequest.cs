using Messager.NET.Requests;

namespace Messager.NET.Autofac.Tests.ContainerBuilders;

internal sealed class TestAutofacRequest : IRequest<string>
{
    public string Invoke() => "pong";
}
