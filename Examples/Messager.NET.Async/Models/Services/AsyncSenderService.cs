using Messager.NET.Async.Models.Events;
using Messager.NET.Senders;
using Microsoft.Extensions.Hosting;

namespace Messager.NET.Async.Models.Services;

public sealed class AsyncSenderService(IAsyncSender<SimpleEvent> sender) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
            await sender.SendAsync(new SimpleEvent(Guid.NewGuid().ToString(), ValueTask.CompletedTask));
        }
    }
}
