using BenchmarkDotNet.Attributes;
using CommunityToolkit.Mvvm.Messaging;
using MessagePipe;
using Messager.NET.Entities;
using Messager.NET.Senders;
using Microsoft.Extensions.DependencyInjection;

namespace Messager.NET.Benchmarks.Benchmarks;

[MemoryDiagnoser]
[CsvExporter]
public class ThroughputEqualityBenchmark
{
	private sealed class BenchmarkMessage
	{
		public int Value { get; set; }
	}

	private sealed class BenchmarkRecipient(ThroughputEqualityBenchmark owner)
	{
		public ThroughputEqualityBenchmark Owner { get; } = owner;
	}

	private event EventHandler<BenchmarkMessage>? StandardEvent;

	private readonly List<IDisposable> _subscriptions = [];
	private readonly List<BenchmarkRecipient> _recipients = [];

	private readonly BenchmarkMessage _message = new();

	private ISender<BenchmarkMessage> _sender = null!;

	private StrongReferenceMessenger _strongMessenger = null!;
	private WeakReferenceMessenger _weakMessenger = null!;

	private IPublisher<BenchmarkMessage> _pipePublisher = null!;
	private ServiceProvider _serviceProvider = null!;

	private int _receivedCount;

	[Params(1, 10, 100)]
	public int SubscriberCount { get; set; }

	[Params(1_000, 10_000, 100_000)]
	public int TotalMessages { get; set; }

	[GlobalSetup]
	public void Setup()
	{
		// Messager
		var exchange = new Exchange();

		_sender = exchange.GetSender<BenchmarkMessage>();
		var receiver = exchange.GetReceiver<BenchmarkMessage>();

		// CommunityToolkit
		_strongMessenger = new StrongReferenceMessenger();
		_weakMessenger = new WeakReferenceMessenger();

		// MessagePipe
		var services = new ServiceCollection();

		services.AddMessagePipe(options => options.EnableAutoRegistration = false);

		_serviceProvider = services.BuildServiceProvider();

		_pipePublisher = _serviceProvider
			.GetRequiredService<IPublisher<BenchmarkMessage>>();

		var pipeSubscriber = _serviceProvider
			.GetRequiredService<ISubscriber<BenchmarkMessage>>();

		for (var i = 0; i < SubscriberCount; i++)
		{
			StandardEvent += OnStandardReceived;

			_subscriptions.Add(receiver.Subscribe(OnReceived));
			_subscriptions.Add(pipeSubscriber.Subscribe(OnReceived));

			var recipient = new BenchmarkRecipient(this);

			_recipients.Add(recipient);

			_strongMessenger.Register<BenchmarkRecipient, BenchmarkMessage>(recipient, static (r, m) => r.Owner.OnReceived(m));
			_weakMessenger.Register<BenchmarkRecipient, BenchmarkMessage>(recipient, static (r, m) => r.Owner.OnReceived(m));
		}

		Verify("Events", () => StandardEvent?.Invoke(this, _message));
		Verify("Messager", () => _sender.Send(_message));
		Verify("Strong", () => _strongMessenger.Send(_message));
		Verify("Weak", () => _weakMessenger.Send(_message));
		Verify("MessagePipe", () => _pipePublisher.Publish(_message));
	}

	[GlobalCleanup]
	public void Cleanup()
	{
		foreach (var subscription in _subscriptions)
			subscription.Dispose();

		_subscriptions.Clear();

		_strongMessenger.Reset();
		_weakMessenger.Reset();

		_recipients.Clear();
		StandardEvent = null;

		_serviceProvider.Dispose();
	}

	[Benchmark(Baseline = true)]
	public int StandardEvents_Throughput()
	{
		_receivedCount = 0;

		for (var i = 0; i < TotalMessages; i++)
		{
			_message.Value = i;
			StandardEvent?.Invoke(this, _message);
		}

		return _receivedCount;
	}

	[Benchmark]
	public int Messager_Throughput()
	{
		_receivedCount = 0;

		for (var i = 0; i < TotalMessages; i++)
		{
			_message.Value = i;
			_sender.Send(_message);
		}

		return _receivedCount;
	}

	[Benchmark]
	public int CommunityToolkit_Strong_Throughput()
	{
		_receivedCount = 0;

		for (var i = 0; i < TotalMessages; i++)
		{
			_message.Value = i;
			_strongMessenger.Send(_message);
		}

		return _receivedCount;
	}

	[Benchmark]
	public int CommunityToolkit_Weak_Throughput()
	{
		_receivedCount = 0;

		for (var i = 0; i < TotalMessages; i++)
		{
			_message.Value = i;
			_weakMessenger.Send(_message);
		}

		return _receivedCount;
	}

	[Benchmark]
	public int MessagePipe_Throughput()
	{
		_receivedCount = 0;

		for (var i = 0; i < TotalMessages; i++)
		{
			_message.Value = i;
			_pipePublisher.Publish(_message);
		}

		return _receivedCount;
	}

	private void Verify(string name, Action publish)
	{
		_receivedCount = 0;

		publish();

		if (_receivedCount != SubscriberCount)
			throw new InvalidOperationException($"{name}: expected {SubscriberCount}, got {_receivedCount}");
	}

	private void OnStandardReceived(object? sender, BenchmarkMessage message) => OnReceived(message);

	private void OnReceived(BenchmarkMessage message) => _receivedCount++;
}
