namespace Messager.NET.Async.Models.Events;

public sealed record SimpleEvent(string Message, ValueTask Task);