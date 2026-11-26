using Messager.NET.Example.Models.Events;
using Messager.NET.Senders;
using Microsoft.Extensions.Hosting;

namespace Messager.NET.Simple.Models.Services;

public sealed class SimpleSenderService : BackgroundService
{
    private readonly ISender<SimpleEvent> _sender;

    public SimpleSenderService(ISender<SimpleEvent> sender)
    {
        _sender = sender;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
            _sender.Send(new SimpleEvent(Guid.NewGuid().ToString()));
        }
    }
}
