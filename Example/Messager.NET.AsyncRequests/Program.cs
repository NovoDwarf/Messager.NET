using Messager.NET.AsyncRequests.Services;
using Messager.NET.DependencyInjection.Extensions;
using Messager.NET.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Messager.NET.AsyncRequests;

public static class Program
{
	public static Task Main(string[] args)
	{
		var builder = Host.CreateApplicationBuilder(args);
		builder.Services.AddMessager(options => options.RequestAssemblies.Add(typeof(AsyncRequestService).Assembly));
		builder.Services.AddSingleton<AsyncRequestService>();
		builder.Services.AddSingleton<AsyncRequestWithInputService>();

		var app = builder.Build();
		app.Services.GetRequiredService<AsyncRequestService>();
		app.Services.GetRequiredService<AsyncRequestWithInputService>();

		return app.RunAsync();
	}
}
