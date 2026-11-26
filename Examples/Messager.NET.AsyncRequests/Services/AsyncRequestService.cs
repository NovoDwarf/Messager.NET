using Messager.NET.Requests;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Messager.NET.AsyncRequests.Services;

public sealed class AsyncRequestService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public AsyncRequestService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _scopeFactory.CreateScope();

            var request = scope.ServiceProvider.GetRequiredService<IAsyncRequest<string>>();
            var result = await request.InvokeAsync();

            Console.WriteLine(result);
            await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
        }
    }
}
