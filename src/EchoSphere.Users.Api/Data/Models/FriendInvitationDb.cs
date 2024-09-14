using EchoSphere.Users.Abstractions.Models;

namespace EchoSphere.Users.Api.Data.Models;

internal sealed class FriendInvitationDb
{
	public required FriendInvitationId Id { get; init; }

	public required UserId FromUserId { get; init; }

	public required UserId ToUserId { get; init; }
}