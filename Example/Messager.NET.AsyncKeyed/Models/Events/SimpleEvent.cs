namespace Messager.NET.AsyncKeyed.Models.Events;

public record struct SimpleEvent(string Message, ValueTask Task);