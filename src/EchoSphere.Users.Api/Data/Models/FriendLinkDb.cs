using EchoSphere.Domain.Abstractions.Models;

namespace EchoSphere.Users.Api.Data.Models;

internal sealed class FriendLinkDb
{
	public required UserId User1Id { get; init; }

	public required UserId User2Id { get; init; }
}