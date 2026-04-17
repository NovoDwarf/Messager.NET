using Messager.NET.Benchmarks.Benchmarks;
using Messager.NET.Extensions;
using Microsoft.Extensions.Hosting;

namespace Messager.NET.Benchmarks;

public static class Program
{
	public static Task Main(string[] args)
	{
		var host = Host.CreateApplicationBuilder(args);
		var provider = new AutofacServiceProviderFactory(ConfigurationAction);
		
		host.ConfigureContainer(provider);
		
		var app = host.Build();
		
		return app.RunAsync();
	}

	private static void ConfigurationAction(ContainerBuilder builder)
	{
		builder.AddMessager();
		
		builder.RegisterType<ThroughputBenchmark>().SingleInstance();
	}
}