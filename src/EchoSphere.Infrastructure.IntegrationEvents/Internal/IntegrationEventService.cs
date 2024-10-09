using EchoSphere.Infrastructure.IntegrationEvents.Abstractions;
using EchoSphere.Infrastructure.IntegrationEvents.Data.Models;
using LinqToDB;

namespace EchoSphere.Infrastructure.IntegrationEvents.Internal;

internal sealed class IntegrationEventService : IIntegrationEventService
{
	private readonly ITable<IntegrationEventDb> _table;
	private readonly IEventSerializer _eventSerializer;

	public IntegrationEventService(IDataContext dataConnection, IEventSerializer eventSerializer)
	{
		_eventSerializer = eventSerializer;
		_table = dataConnection.GetTable<IntegrationEventDb>();
	}

	public Task PublishEvent<T>(T @event, CancellationToken cancellationToken)
		where T : class, IIntegrationEvent
	{
		var serializedIntegrationEvent = _eventSerializer.Serialize(@event);

		return _table.InsertAsync(
			() => new IntegrationEventDb
			{
				TypeName = serializedIntegrationEvent.TypeName,
				EventData = serializedIntegrationEvent.EventData,
				State = IntegrationEventState.Initial,
			},
			cancellationToken);
	}
}