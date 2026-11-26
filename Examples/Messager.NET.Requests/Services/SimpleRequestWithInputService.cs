using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Messager.NET.Requests.Services;

public sealed class SimpleRequestWithInputService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public SimpleRequestWithInputService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _scopeFactory.CreateScope();

            var request = scope.ServiceProvider.GetRequiredService<IRequest<string, string>>();
            var result = request.Invoke(DateTime.Now.ToString(CultureInfo.CurrentCulture));

            Console.WriteLine(result);
            await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
        }
    }
}
