using EchoSphere.Domain.Abstractions.Models;

namespace EchoSphere.Messages.Api.Data.Models;

internal sealed class ChatParticipantDb
{
	public required ChatId ChatId { get; init; }

	public required UserId UserId { get; init; }
}