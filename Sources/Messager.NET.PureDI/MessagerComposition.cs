using Messager.NET.Entities;
using Messager.NET.Factories;
using Messager.NET.Models.PubSub.Receivers;
using Messager.NET.Models.PubSub.Senders;
using Messager.NET.Receivers;
using Messager.NET.Senders;
using Pure.DI;

namespace Messager.NET.PureDI;

internal static class MessagerBindings
{
    private static void Setup()
    {
        DI.Setup("Messager.NET.PureDI.MessagerBindings", CompositionKind.Internal)
            .Bind<Exchange>().Bind<IBrokerFactory>().Bind<IKeyedBrokerFactory>()
            .As(Lifetime.Singleton).To<Exchange>()
            .Bind<ISender<TT>>().To<Sender<TT>>()
            .Bind<IReceiver<TT>>().To<Receiver<TT>>()
            .Bind<IAsyncSender<TT>>().To<AsyncSender<TT>>()
            .Bind<IAsyncReceiver<TT>>().To<AsyncReceiver<TT>>()
            .Bind<ISender<TT1, TT2>>().To<KeyedSender<TT1, TT2>>()
            .Bind<IReceiver<TT1, TT2>>().To<KeyedReceiver<TT1, TT2>>()
            .Bind<IAsyncSender<TT1, TT2>>().To<AsyncKeyedSender<TT1, TT2>>()
            .Bind<IAsyncReceiver<TT1, TT2>>().To<AsyncKeyedReceiver<TT1, TT2>>();
    }
}
