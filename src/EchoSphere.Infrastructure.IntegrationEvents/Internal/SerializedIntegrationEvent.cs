namespace EchoSphere.Infrastructure.IntegrationEvents.Internal;

internal readonly struct SerializedIntegrationEvent
{
	public required string TypeName { get; init; }

	public required byte[] EventData { get; init; }
}