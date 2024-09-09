using EchoSphere.Users.Abstractions.Models;

namespace EchoSphere.Users.Api.Data.Models;

internal sealed class FollowerDb
{
	public required UserId UserId { get; init; }

	public required UserId FollowerUserId { get; init; }
}