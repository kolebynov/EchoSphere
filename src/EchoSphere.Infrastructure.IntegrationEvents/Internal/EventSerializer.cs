using System.Text.Json;
using EchoSphere.Domain.Abstractions.Extensions;

namespace EchoSphere.Infrastructure.IntegrationEvents.Internal;

internal sealed class EventSerializer : IEventSerializer
{
	private readonly JsonSerializerOptions _serializerOptions =
		new JsonSerializerOptions(JsonSerializerDefaults.General).AddDomainConverters();

	public SerializedIntegrationEvent Serialize<T>(T @event)
		where T : class, IIntegrationEvent
	{
		var eventType = @event.GetType();
		return new()
		{
			TypeName = $"{eventType.FullName}, {eventType.Assembly.GetName().Name}",
			EventData = JsonSerializer.SerializeToUtf8Bytes(@event, _serializerOptions),
		};
	}

	public IIntegrationEvent Deserialize(SerializedIntegrationEvent serializedIntegrationEvent)
	{
		var eventType = Type.GetType(serializedIntegrationEvent.TypeName, true)!;
		return (IIntegrationEvent)JsonSerializer.Deserialize(serializedIntegrationEvent.EventData, eventType,
			_serializerOptions)!;
	}
}