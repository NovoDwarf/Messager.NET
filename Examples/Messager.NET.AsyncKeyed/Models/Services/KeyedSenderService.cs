using Messager.NET.AsyncKeyed.Models.Events;
using Messager.NET.Senders;
using Microsoft.Extensions.Hosting;

namespace Messager.NET.AsyncKeyed.Models.Services;

public sealed class KeyedSenderService(IAsyncSender<string, SimpleEvent> sender) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
            await sender.SendAsync("key", new SimpleEvent(Guid.NewGuid().ToString(), ValueTask.CompletedTask));
            await sender.SendAsync("another-key", new SimpleEvent(Guid.NewGuid().ToString(), ValueTask.CompletedTask));
        }
    }
}
