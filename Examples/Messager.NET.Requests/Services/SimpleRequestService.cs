using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Messager.NET.Requests.Services;

public sealed class SimpleRequestService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public SimpleRequestService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _scopeFactory.CreateScope();

            var request = scope.ServiceProvider.GetRequiredService<IRequest<string>>();
            var result = request.Invoke();

            Console.WriteLine(result);
            await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
        }
    }
}
