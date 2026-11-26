using Messager.NET.Microsoft.DependencyInjection.Extensions;
using Messager.NET.Requests.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Messager.NET.Requests;

public static class Program
{
	public static Task Main(string[] args)
	{
		var builder = Host.CreateApplicationBuilder(args);

		builder.Services.AddMessager(options => options.AddRequestsFromAssemblyOf(typeof(SimpleRequestService).Assembly));
		builder.Services.AddHostedService<SimpleRequestService>();
		builder.Services.AddHostedService<SimpleRequestWithInputService>();

		var app = builder.Build();


		return app.RunAsync();
	}
}
