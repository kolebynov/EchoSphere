using EchoSphere.Domain.Abstractions.Models;

namespace EchoSphere.Messages.Abstractions.Models;

public sealed class ChatMessage
{
	public required MessageId Id { get; init; }

	public required DateTimeOffset Timestamp { get; init; }

	public required UserId SenderId { get; init; }

	public required string Text { get; init; }
}