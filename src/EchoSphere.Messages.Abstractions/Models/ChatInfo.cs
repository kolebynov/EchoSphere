using EchoSphere.Users.Abstractions.Models;

namespace EchoSphere.Messages.Abstractions.Models;

public sealed class ChatInfo
{
	public required ChatId Id { get; init; }

	public required IReadOnlyList<UserId> Participants { get; init; }
}