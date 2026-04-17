using Messager.NET.Async.Models.Services;
using Messager.NET.Extensions;
using Messager.NET.Microsoft.DependencyInjection.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Messager.NET.Async;

public class Program
{
	public static Task Main(string[] args)
	{
		var builder = Host.CreateApplicationBuilder(args);
		builder.Services.AddMessager();
		builder.Services.AddSingleton<AsyncReceiverService>();
		builder.Services.AddSingleton<AsyncSenderService>();

		var app = builder.Build();
		app.Services.GetRequiredService<AsyncReceiverService>();
		app.Services.GetRequiredService<AsyncSenderService>();

		return app.RunAsync();
	}
}
