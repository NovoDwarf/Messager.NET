using Messager.NET.Queues;

namespace Messager.NET.Queued;

internal static class Program
{
    private static async Task Main()
    {
        await using var queue = new MessageQueue<WorkItem>(ProcessAsync, capacity: 2);
        
        IMessageQueue<WorkItem> producer = queue;

        for (var i = 1; i <= 5; i++)
        {
            await producer.EnqueueAsync(new WorkItem(i, $"Work item {i}"));
            Console.WriteLine($"Accepted: {i}");
        }

        await queue.CompleteAsync();
        Console.WriteLine("All 5 work items processed.");
    }

    private static async ValueTask ProcessAsync(WorkItem message)
    {
        await Task.Delay(25);
        Console.WriteLine($"Processed: {message.Id} — {message.Description}");
    }
}
