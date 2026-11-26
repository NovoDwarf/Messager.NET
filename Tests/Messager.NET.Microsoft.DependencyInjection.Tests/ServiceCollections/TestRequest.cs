using Messager.NET.Requests;

namespace Messager.NET.Microsoft.DependencyInjection.Tests.ServiceCollections;

internal sealed class TestRequest : IRequest<string>
{
    public string Invoke() => "pong";
}
