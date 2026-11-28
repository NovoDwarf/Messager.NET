namespace Messager.NET.PureDI;

internal static class Program
{
	public static void Main()
	{
		var composition = new Models.Composition();
		var messaging = composition.Messaging;

		using var subscription = messaging.Receiver.Subscribe(Console.WriteLine);
		
		messaging.Sender.Send("Hello from Pure.DI!");
		Console.WriteLine($"Request result: {composition.StringLength.Invoke("Pure.DI")}");
	}
}