using Messager.NET.Receivers;
using Messager.NET.Senders;

namespace Messager.NET.PureDI.Models;

public sealed class MessagingDemo
{
    public MessagingDemo(
        ISender<string> sender,
        IReceiver<string> receiver,
        IAsyncSender<string> asyncSender,
        IAsyncReceiver<string> asyncReceiver,
        ISender<string, string> keyedSender,
        IReceiver<string, string> keyedReceiver,
        IAsyncSender<string, string> asyncKeyedSender,
        IAsyncReceiver<string, string> asyncKeyedReceiver)
    {
        Sender = sender;
        Receiver = receiver;
        AsyncSender = asyncSender;
        AsyncReceiver = asyncReceiver;
        KeyedSender = keyedSender;
        KeyedReceiver = keyedReceiver;
        AsyncKeyedSender = asyncKeyedSender;
        AsyncKeyedReceiver = asyncKeyedReceiver;
    }

    public ISender<string> Sender { get; }

    public IReceiver<string> Receiver { get; }

    public IAsyncSender<string> AsyncSender { get; }

    public IAsyncReceiver<string> AsyncReceiver { get; }

    public ISender<string, string> KeyedSender { get; }

    public IReceiver<string, string> KeyedReceiver { get; }

    public IAsyncSender<string, string> AsyncKeyedSender { get; }

    public IAsyncReceiver<string, string> AsyncKeyedReceiver { get; }
}
