using Messager.NET.Factories;
using Messager.NET.Receivers;
using Messager.NET.Requests;
using Messager.NET.Senders;
using Pure.DI;

namespace Messager.NET.PureDI.Models;

public partial class Composition
{
	private static void Setup()
	{
		DI.Setup(nameof(Composition))
		  .Hint(Hint.Resolve, "Off")
		  .DependsOn("Messager.NET.PureDI.MessagerBindings")
		  .Bind<IRequest<int, string>>().To<StringLengthRequest>()
		  .Root<MessagingDemo>("Messaging")
		  .Root<IBrokerFactory>("BrokerFactory")
		  .Root<IKeyedBrokerFactory>("KeyedBrokerFactory")
		  .Root<ISender<int>>("NumberSender")
		  .Root<IReceiver<int>>("NumberReceiver")
		  .Root<IRequest<int, string>>("StringLength");
	}
}