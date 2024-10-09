using EchoSphere.Domain.Abstractions.Models;

namespace EchoSphere.Users.Abstractions.Models;

public sealed class FriendInvitation
{
	public required FriendInvitationId Id { get; init; }

	public required UserId FromUserId { get; init; }
}