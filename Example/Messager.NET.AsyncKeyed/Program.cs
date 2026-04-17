using Messager.NET.AsyncKeyed.Models.Services;
using Messager.NET.Extensions;
using Messager.NET.Microsoft.DependencyInjection.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Messager.NET.AsyncKeyed;

public static class Program
{
	public static Task Main(string[] args)
	{
		var builder = Host.CreateApplicationBuilder(args);
		builder.Services.AddMessager();
		builder.Services.AddSingleton<KeyedReceiverService>();
		builder.Services.AddSingleton<KeyedSenderService>();

		var app = builder.Build();
		app.Services.GetRequiredService<KeyedReceiverService>();
		app.Services.GetRequiredService<KeyedSenderService>();

		return app.RunAsync();
	}
}
