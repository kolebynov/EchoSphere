using EchoSphere.Users.Abstractions.Models;

namespace EchoSphere.Users.Abstractions;

public interface IFollowService
{
	ValueTask Follow(UserId followerUserId, UserId followUserId, CancellationToken cancellationToken);

	ValueTask<IReadOnlyList<UserId>> GetFollowers(UserId userId, CancellationToken cancellationToken);
}