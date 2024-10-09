using EchoSphere.Infrastructure.IntegrationEvents;

namespace EchoSphere.Messages.Api.Events;

public sealed class MessageSentEvent : IIntegrationEvent
{
	public required Guid ChatId { get; init; }

	public required Guid SenderId { get; init; }
}