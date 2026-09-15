using Messager.NET.Example.Models.Services;
using Messager.NET.Extensions;
using Messager.NET.Microsoft.DependencyInjection.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Messager.NET.Example;

public class Program
{
	public static Task Main(string[] args)
	{
		var builder = Host.CreateApplicationBuilder(args);
		builder.Services.AddMessager();
		builder.Services.AddSingleton<SimpleReceiverService>();
		builder.Services.AddSingleton<SimpleSenderService>();

		var app = builder.Build();
		app.Services.GetRequiredService<SimpleReceiverService>();
		app.Services.GetRequiredService<SimpleSenderService>();

		return app.RunAsync();
	}
}
