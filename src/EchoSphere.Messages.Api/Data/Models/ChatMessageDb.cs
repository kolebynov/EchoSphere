using EchoSphere.Domain.Abstractions.Models;

namespace EchoSphere.Messages.Api.Data.Models;

internal sealed class ChatMessageDb
{
	public MessageId Id { get; init; }

	public required DateTimeOffset Timestamp { get; init; }

	public required ChatId ChatId { get; init; }

	public required UserId SenderId { get; init; }

	public required string Text { get; init; }
}