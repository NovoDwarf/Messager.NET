namespace Messager.NET.AsyncKeyed.Models.Events;

public sealed record SimpleEvent(string Message, ValueTask Task);