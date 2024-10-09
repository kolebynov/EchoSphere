namespace EchoSphere.Infrastructure.IntegrationEvents.Data.Models;

internal sealed class IntegrationEventDb
{
	public long Id { get; init; }

	public string TypeName { get; init; } = null!;

	public byte[] EventData { get; init; } = null!;

	public required IntegrationEventState State { get; init; }
}