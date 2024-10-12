using Confluent.Kafka;
using EchoSphere.Infrastructure.Hosting;
using EchoSphere.Infrastructure.IntegrationEvents.Data.Models;
using EchoSphere.Infrastructure.IntegrationEvents.Settings;
using LinqToDB;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EchoSphere.Infrastructure.IntegrationEvents.Internal;

internal sealed class ProduceIntegrationEventHostedService : BaseHostedService
{
	private readonly IProducer<Null, SerializedIntegrationEvent> _producer;
	private readonly IntegrationEventsSettings _integrationEventsSettings;
	private readonly ILogger<ProduceIntegrationEventHostedService> _logger;
	private ITable<IntegrationEventDb> _eventsTable = null!;

	public ProduceIntegrationEventHostedService(
		IServiceScopeFactory serviceScopeFactory, IProducer<Null, SerializedIntegrationEvent> producer,
		IOptions<IntegrationEventsSettings> integrationEventsSettings,
		ILogger<ProduceIntegrationEventHostedService> logger)
		: base(serviceScopeFactory)
	{
		_producer = producer;
		_logger = logger;
		_integrationEventsSettings = integrationEventsSettings.Value;
	}

	protected override async Task RunAsync(IServiceProvider scopeServiceProvider, CancellationToken stopCancellationToken)
	{
		_eventsTable = scopeServiceProvider.GetRequiredService<IDataContext>().GetTable<IntegrationEventDb>();

		while (!stopCancellationToken.IsCancellationRequested)
		{
			var needDelay = await ProcessEvents(stopCancellationToken);
			if (needDelay)
			{
				await Task.Delay(TimeSpan.FromSeconds(1), stopCancellationToken);
			}
		}
	}

	private async Task<bool> ProcessEvents(CancellationToken stopCancellationToken)
	{
		var events = await _eventsTable
			.Where(x => x.State == IntegrationEventState.Pending)
			.OrderBy(x => x.Id)
			.Take(_integrationEventsSettings.BatchSize)
			.ToArrayAsync(stopCancellationToken);

		if (events.Length == 0)
		{
			return true;
		}

		var publishedEventIds = await PublishEvents(events);
		_logger.LogInformation(
			"Integration events published. [Count: {Count}][Successful: {Successful}]", events.Length, publishedEventIds.Count);

		await _eventsTable.UpdateAsync(
			x => publishedEventIds.Contains(x.Id), x => new IntegrationEventDb { State = IntegrationEventState.Processed },
			stopCancellationToken);

		return events.Length < _integrationEventsSettings.BatchSize && publishedEventIds.Count == events.Length;
	}

	private Task<IReadOnlyCollection<long>> PublishEvents(IntegrationEventDb[] integrationEventsDb)
	{
		var tcs = new TaskCompletionSource<IReadOnlyCollection<long>>();
		if (integrationEventsDb.Length == 1)
		{
			PublishSingleEvent(integrationEventsDb[0], tcs);
		}
		else
		{
			PublishMultipleEvents(integrationEventsDb, tcs);
		}

		return tcs.Task;
	}

	private void PublishSingleEvent(
		IntegrationEventDb integrationEventDb, TaskCompletionSource<IReadOnlyCollection<long>> tcs)
	{
		_producer.Produce(
			_integrationEventsSettings.TopicName, ToKafkaMessage(integrationEventDb),
			deliveryResult =>
			{
				tcs.SetResult(deliveryResult.Status == PersistenceStatus.Persisted ? [integrationEventDb.Id] : []);
			});
	}

	private void PublishMultipleEvents(
		IntegrationEventDb[] integrationEventsDb, TaskCompletionSource<IReadOnlyCollection<long>> tcs)
	{
		var deliveryReports = new (DeliveryReport<Null, SerializedIntegrationEvent>, long)[integrationEventsDb.Length];
		var publishedEventCount = 0;

		foreach (var integrationEventDb in integrationEventsDb)
		{
			_producer.Produce(
				_integrationEventsSettings.TopicName, ToKafkaMessage(integrationEventDb),
				deliveryResult =>
				{
					var published = Interlocked.Increment(ref publishedEventCount);
					deliveryReports[published - 1] = (deliveryResult, integrationEventDb.Id);

					if (published == deliveryReports.Length)
					{
						tcs.SetResult(deliveryReports
							.Where(tuple => tuple.Item1.Status == PersistenceStatus.Persisted)
							.Select(tuple => tuple.Item2)
							.ToArray());
					}
				});
		}
	}

	private static Message<Null, SerializedIntegrationEvent> ToKafkaMessage(IntegrationEventDb integrationEventDb) =>
		new Message<Null, SerializedIntegrationEvent>
		{
			Value = new SerializedIntegrationEvent
			{
				TypeName = integrationEventDb.TypeName,
				EventData = integrationEventDb.EventData,
			},
		};
}