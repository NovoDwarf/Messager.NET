using Messager.NET.DependencyInjection.Extensions;
using Messager.NET.Extensions;
using Messager.NET.Requests.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Messager.NET.Requests;

public class Program
{
	public static Task Main(string[] args)
	{
		var builder = Host.CreateApplicationBuilder(args);
		builder.Services.AddMessager(options => options.RequestAssemblies.Add(typeof(SimpleRequestService).Assembly));
		builder.Services.AddSingleton<SimpleRequestService>();
		builder.Services.AddSingleton<SimpleRequestWithInputService>();

		var app = builder.Build();
		app.Services.GetRequiredService<SimpleRequestService>();
		app.Services.GetRequiredService<SimpleRequestWithInputService>();

		return app.RunAsync();
	}
}
