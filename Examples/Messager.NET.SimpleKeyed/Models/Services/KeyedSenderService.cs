using Messager.NET.SimpleKeyed.Models.Events;
using Messager.NET.Senders;
using Microsoft.Extensions.Hosting;

namespace Messager.NET.SimpleKeyed.Models.Services;

public sealed class KeyedSenderService : BackgroundService
{
    private readonly ISender<string, SimpleEvent> _sender;

    public KeyedSenderService(ISender<string, SimpleEvent> sender)
    {
        _sender = sender;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);

            _sender.Send("key", new SimpleEvent(Guid.NewGuid().ToString()));
            _sender.Send("another-key", new SimpleEvent(Guid.NewGuid().ToString()));
        }
    }
}
