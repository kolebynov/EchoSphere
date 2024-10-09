using System.Text.Json;

namespace EchoSphere.Infrastructure.IntegrationEvents.Internal;

internal sealed class EventSerializer : IEventSerializer
{
	private readonly JsonSerializerOptions _serializerOptions = new(JsonSerializerDefaults.General);

	public SerializedIntegrationEvent Serialize<T>(T @event)
		where T : class, IIntegrationEvent =>
		new()
		{
			TypeName = @event.GetType().AssemblyQualifiedName!,
			EventData = JsonSerializer.SerializeToUtf8Bytes(@event, _serializerOptions),
		};

	public IIntegrationEvent Deserialize(SerializedIntegrationEvent serializedIntegrationEvent)
	{
		var eventType = Type.GetType(serializedIntegrationEvent.TypeName, true)!;
		return (IIntegrationEvent)JsonSerializer.Deserialize(serializedIntegrationEvent.EventData, eventType,
			_serializerOptions)!;
	}
}