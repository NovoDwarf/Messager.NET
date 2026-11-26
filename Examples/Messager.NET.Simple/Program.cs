using Messager.NET.Microsoft.DependencyInjection.Extensions;
using Messager.NET.Simple.Models.Services;
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
		builder.Services.AddHostedService<SimpleSenderService>();

		var app = builder.Build();
		app.Services.GetRequiredService<SimpleReceiverService>();

		return app.RunAsync();
	}
}
