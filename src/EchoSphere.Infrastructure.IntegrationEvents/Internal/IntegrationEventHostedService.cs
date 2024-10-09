using System.Linq.Expressions;
using EchoSphere.Infrastructure.Hosting;
using EchoSphere.Infrastructure.IntegrationEvents.Data.Models;
using LinqToDB;
using Microsoft.Extensions.DependencyInjection;

namespace EchoSphere.Infrastructure.IntegrationEvents.Internal;

internal sealed class IntegrationEventHostedService : BaseHostedService
{
	public IntegrationEventHostedService(IServiceScopeFactory serviceScopeFactory)
		: base(serviceScopeFactory)
	{
	}

	protected override async Task RunAsync(IServiceProvider scopeServiceProvider, CancellationToken stopCancellationToken)
	{
		var eventSerializer = scopeServiceProvider.GetRequiredService<IEventSerializer>();
		var dataConnection = scopeServiceProvider.GetRequiredService<IDataContext>();
		var eventsTable = dataConnection.GetTable<IntegrationEventDb>();

		await ResetInProgressEvents(eventsTable, stopCancellationToken);

		while (!stopCancellationToken.IsCancellationRequested)
		{
			await Task.Delay(TimeSpan.FromSeconds(1), stopCancellationToken);

			var events = await eventsTable
				.Where(x => x.State == IntegrationEventState.Initial)
				.OrderBy(x => x.Id)
				.Take(50)
				.ToArrayAsync(stopCancellationToken);
			if (events.Length == 0)
			{
				continue;
			}

			var firstId = events[0].Id;
			var lastId = events[^1].Id;

			Expression<Func<IntegrationEventDb, bool>> currentEventsPredicate = x => x.Id >= firstId && x.Id <= lastId;

			await eventsTable.UpdateAsync(
				currentEventsPredicate, x => new IntegrationEventDb { State = IntegrationEventState.Processing },
				stopCancellationToken);

			var eventsDeserialized = events
				.Select(x => eventSerializer.Deserialize(
					new SerializedIntegrationEvent { TypeName = x.TypeName, EventData = x.EventData }))
				.ToArray();

			await ProcessEvents(eventsDeserialized, stopCancellationToken);

			await eventsTable.UpdateAsync(
				currentEventsPredicate, x => new IntegrationEventDb { State = IntegrationEventState.Processed },
				stopCancellationToken);
		}
	}

	private static Task ProcessEvents(IIntegrationEvent[] integrationEvents, CancellationToken cancellationToken)
	{
		// TODO: Process events
		return Task.Delay(TimeSpan.FromMilliseconds(100), cancellationToken);
	}

	private static Task ResetInProgressEvents(ITable<IntegrationEventDb> table, CancellationToken cancellationToken) =>
		table.UpdateAsync(
			x => x.State == IntegrationEventState.Processing,
			x => new IntegrationEventDb { State = IntegrationEventState.Initial },
			cancellationToken);
}