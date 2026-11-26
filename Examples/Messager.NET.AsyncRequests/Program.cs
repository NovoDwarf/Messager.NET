using Messager.NET.AsyncRequests.Services;
using Messager.NET.Microsoft.DependencyInjection.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Messager.NET.AsyncRequests;

public static class Program
{
	public static Task Main(string[] args)
	{
		var builder = Host.CreateApplicationBuilder(args);

		builder.Services.AddMessager(options => options.AddRequestsFromAssemblyOf(typeof(AsyncRequestService).Assembly));
		builder.Services.AddHostedService<AsyncRequestService>();
		builder.Services.AddHostedService<AsyncRequestWithInputService>();

		var app = builder.Build();


		return app.RunAsync();
	}
}
