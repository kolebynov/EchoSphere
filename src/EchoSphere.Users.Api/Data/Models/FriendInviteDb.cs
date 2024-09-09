using EchoSphere.Users.Abstractions.Models;

namespace EchoSphere.Users.Api.Data.Models;

internal sealed class FriendInviteDb
{
	public required UserId FromUserId { get; init; }

	public required UserId ToUserId { get; init; }
}