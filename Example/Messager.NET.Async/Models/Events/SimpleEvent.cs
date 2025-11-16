namespace Messager.NET.Async.Models.Events;

public record struct SimpleEvent(string Message, ValueTask Task);