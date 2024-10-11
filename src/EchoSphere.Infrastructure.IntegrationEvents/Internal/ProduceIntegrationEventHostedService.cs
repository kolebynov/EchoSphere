using Confluent.Kafka;
using EchoSphere.Infrastructure.Hosting;
using EchoSphere.Infrastructure.IntegrationEvents.Data.Models;
using EchoSphere.Infrastructure.IntegrationEvents.Settings;
using LinqToDB;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace EchoSphere.Infrastructure.IntegrationEvents.Internal;

internal sealed class ProduceIntegrationEventHostedService : BaseHostedService
{
	private readonly IProducer<Null, SerializedIntegrationEvent> _producer;
	private readonly IntegrationEventsSettings _integrationEventsSettings;
	private ITable<IntegrationEventDb> _eventsTable = null!;

	public ProduceIntegrationEventHostedService(
		IServiceScopeFactory serviceScopeFactory, IProducer<Null, SerializedIntegrationEvent> producer,
		IOptions<IntegrationEventsSettings> integrationEventsSettings)
		: base(serviceScopeFactory)
	{
		_producer = producer;
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

		var processedEventIds = await SendEvents(events, stopCancellationToken);

		await _eventsTable.UpdateAsync(
			x => processedEventIds.Contains(x.Id), x => new IntegrationEventDb { State = IntegrationEventState.Processed },
			stopCancellationToken);

		return events.Length < _integrationEventsSettings.BatchSize && processedEventIds.Length == events.Length;
	}

	private async Task<long[]> SendEvents(IntegrationEventDb[] integrationEventsDb, CancellationToken cancellationToken)
	{
		var tasks = integrationEventsDb
			.Select(async integrationEventDb =>
			{
				var serializedIntegrationEvent = new SerializedIntegrationEvent
				{
					TypeName = integrationEventDb.TypeName,
					EventData = integrationEventDb.EventData,
				};

				var deliveryResult = await _producer.ProduceAsync(
					_integrationEventsSettings.TopicName,
					new Message<Null, SerializedIntegrationEvent> { Value = serializedIntegrationEvent },
					cancellationToken);

				return (integrationEventDb.Id, deliveryResult.Status);
			});

		var results = await Task.WhenAll(tasks);
		return results.Where(x => x.Status == PersistenceStatus.Persisted).Select(x => x.Id).ToArray();
	}
}