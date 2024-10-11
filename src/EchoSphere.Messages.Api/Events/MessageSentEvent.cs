using EchoSphere.Domain.Abstractions.Models;
using EchoSphere.Infrastructure.IntegrationEvents;

namespace EchoSphere.Messages.Api.Events;

public sealed class MessageSentEvent : IIntegrationEvent
{
	public required ChatId ChatId { get; init; }

	public required UserId SenderId { get; init; }
}