namespace EchoSphere.Infrastructure.IntegrationEvents.Internal;

internal interface IEventSerializer
{
	SerializedIntegrationEvent Serialize<T>(T @event)
		where T : class, IIntegrationEvent;

	IIntegrationEvent Deserialize(SerializedIntegrationEvent serializedIntegrationEvent);
}